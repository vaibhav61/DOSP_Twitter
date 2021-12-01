#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
#r "nuget: Akka.Remote"
#r "nuget: Newtonsoft.Json"
#r "nuget: WebSocketSharp, 1.0.3-rc11"
#r "nuget: MathNet.Numerics" 
#r "nuget: Akka.Serialization.Hyperion"
#load @"./types.fsx"

open System
open System.Threading
open System.Diagnostics
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Types
open System.Collections.Generic
open System.Text.RegularExpressions

let config = 
    ConfigurationFactory.ParseString(
        @"akka {
             actor.serializers{
              json  = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
              bytes = ""Akka.Serialization.ByteArraySerializer""
            }
             actor.serialization-bindings {
              ""System.Byte[]"" = bytes
              ""System.Object"" = json
            
            }
           actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                
            }
            remote.helios.tcp {
               hostname = ""localhost""
                port = 8000
            }
        }")
let userDictionary = new Dictionary<string, ActorSelection>() 
let tweetsDictionary = new Dictionary<string, List<string>>() 
let subscriberDictionary = new Dictionary<string, List<string>>() 
let hashtagDictionary = new Dictionary<string, List<string>>() 
let userHashtagDictionary = new Dictionary<string,List<string>>() 
let mentionDictionary = new Dictionary<string, List<string>>() 
let subscribedToTweetsDictionary = new Dictionary<string, List<string>>()

let system = ActorSystem.Create("Twitter",config)

let initializeClient(msg: recordRegistration) = 
      let objectRegister = msg
      let clientId = objectRegister.ClientID
      let clientObject = objectRegister.ActorObj
      userDictionary.Add(clientId,clientObject)
      let subscriberList = new List<string>()
      let tweetsList = new List<string>()
      let tweetsSubscribedToList = new List<string>()
      let mentionedTweetsList = new List<string>()
      let hashtagUserList = new List<string>()
      subscriberDictionary.Add(clientId,subscriberList)
      tweetsDictionary.Add(clientId,tweetsList)
      subscribedToTweetsDictionary.Add(clientId,tweetsSubscribedToList)
      mentionDictionary.Add(clientId,mentionedTweetsList)
      userHashtagDictionary.Add(clientId,hashtagUserList)
      //printfn "%s Registered" clientId
      let registerRetObj = {recordDefaultRegistration with Message = "Registered"}
      clientObject <! registerRetObj


let subscribe(msg: addSubscribedUsers) = 
      let clientId = msg.ClientName
      let subscriberName = msg.SubscriberName
      let key,actorSelectObject = userDictionary.TryGetValue clientId
      let client,subscriberList = subscriberDictionary.TryGetValue clientId
      if not(subscriberList.Contains(subscriberName)) then //add the user to the subscriber list if he is not there
        subscriberList.Add(subscriberName)
      let subscriberRetObj = {msg with Message = "Subscribed"} 
      //printfn "%s subscribed to %s" subscriberName clientId
      actorSelectObject <! subscriberRetObj 


let recordTweets(msg: recordTweets) = 
               let tweet = msg.Tweet
               let clientTweeting = msg.ClientName 
               let words = tweet.Split [|' '|]
               let mutable messageType = "NormalTweet"
               let clientsMentionedList = new List<string>()
               
               for word in words do
               //if it's a mention then add the mentioned users to the list
                  if Regex.IsMatch(word, "^(@)([a-zA-Z])+") then
                     clientsMentionedList.Add(word.Remove(0,1))
                     messageType <- "Mentions"
                 //if it's a hashtag then add the hashtag and the tweet to the hashTagsDict and hashTagsUserDict    
                  else if Regex.IsMatch(word, "^(#)([a-zA-Z0-9])+") then
                     let hashTag = word
                     if not(hashtagDictionary.ContainsKey(hashTag)) then
                        hashtagDictionary.Add(hashTag,new List<string>())
                     let htkey,hashTagList = hashtagDictionary.TryGetValue hashTag
                     hashTagList.Add(tweet)
                     let htUserKey,hashTagUserList = userHashtagDictionary.TryGetValue clientTweeting
                     hashTagUserList.Add(tweet)

               //add this tweet to the user's tweetList in tweetsDictionary                           
               let key,tweetsList = tweetsDictionary.TryGetValue clientTweeting
               tweetsList.Add(tweet)  

               match messageType with
               | "NormalTweet" -> 
             //send the tweet to all the subscribers and add this tweet to the list of tweets the subscribers have subscribed to
                     let key1,subscriberList =  subscriberDictionary.TryGetValue clientTweeting
                     for subscriber in subscriberList do
                        let tweetToBeSent = {msg with Message = "GetTweets"}
                        let key2,actorSelectObject = userDictionary.TryGetValue subscriber
                        let key3,subscribedToTweetsList = subscribedToTweetsDictionary.TryGetValue subscriber 
                        subscribedToTweetsList.Add(tweet)
                       // //printfn "%A" subscribedToTweetsList
                        actorSelectObject <! tweetToBeSent
                        //printfn "%s's Tweet \"%s\" sent to %s"  clientTweeting tweet subscriber
                | "Mentions" -> //send tweets to only the mentioned users
                     for mentionedClient in  clientsMentionedList do
                        let mentionKey,mentionedList = mentionDictionary.TryGetValue mentionedClient
                        let key2,actorSelectObject = userDictionary.TryGetValue mentionedClient
                        mentionedList.Add(tweet)
                        let sendMentionTweet = {msg with Message = "GetMentionedTweets"}
                        actorSelectObject <! sendMentionTweet
                        //printfn "%s has been mentioned in Tweet \"%s\" of %s. Thus this tweet sent to %s"  mentionedClient tweet clientTweeting mentionedClient

               | _->()
let recordRetweet(msg: recordRetweets) = 
               let retweetingClient = msg.ClientName
               let random = System.Random()
               let key,subscribedTweetsList = subscribedToTweetsDictionary.TryGetValue retweetingClient
               let subscribedTweetsArray = subscribedTweetsList.ToArray()
               let length =  subscribedTweetsArray.Length
               if length > 0 then
                 let randomIndex = random.Next(0,length)
                 let mutable tweet = subscribedTweetsArray.[randomIndex] 
                 tweet <- "Retweet by " + retweetingClient + " :: " + tweet
                 let recordTweets = {ClientName = retweetingClient;Message = "Tweets"; Tweet=tweet}
                 let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                 serverEngine <! recordTweets

let recordQuery(msg: recordQuery) = 
               let message = msg.Message
               let clientName = msg.ClientName
               let key2,actorSelectObject = userDictionary.TryGetValue clientName
               match message with
               | "ReturnAllSubscribedTweets" ->      
                    let key,subscribedToTweetsList = subscribedToTweetsDictionary.TryGetValue clientName
                    let sendsubscribedToList = subscribedToTweetsList
                    let recordQuery = {msg with TweetsList = sendsubscribedToList; Message = "GetTweetsSubscribedTo"}
                    actorSelectObject <! recordQuery
               | "ReturnAllTweetsWithHashtags" ->
                    let hashtag = msg.HashTag
                    let key,hashtagsList = hashtagDictionary.TryGetValue hashtag
                    let recordQuery = {msg with TweetsList = hashtagsList; Message = "GetTweetsWithHashTags"}
                    actorSelectObject <! recordQuery
               | "ReturnTweetsWithMentions" ->
                    let key,mentionedList = mentionDictionary.TryGetValue clientName
                    let recordQuery = {msg with TweetsList = mentionedList; Message = "GetMentionedTweets"}
                    actorSelectObject <! recordQuery      
               | _-> ()     
type Server () =
    inherit Actor()
    
    override x.OnReceive(msg) =  
 
        match msg with 
        | :? recordRegistration as msg  -> //registering a client in the server
               let message = msg.Message
               match message with 
               

               | "Initialize Client" -> 
                    initializeClient(msg)
                    //  let objectRegister = msg
                    //  let clientId = objectRegister.ClientID
                    //  let clientObject = objectRegister.ActorObj
                    //  userDictionary.Add(clientId,clientObject)
                    //  let subscriberList = new List<string>()
                    //  let tweetsList = new List<string>()
                    //  let tweetsSubscribedToList = new List<string>()
                    //  let mentionedTweetsList = new List<string>()
                    //  let hashtagUserList = new List<string>()
                    //  subscriberDictionary.Add(clientId,subscriberList)
                    //  tweetsDictionary.Add(clientId,tweetsList)
                    //  subscribedToTweetsDictionary.Add(clientId,tweetsSubscribedToList)
                    //  mentionDictionary.Add(clientId,mentionedTweetsList)
                    //  userHashtagDictionary.Add(clientId,hashtagUserList)
                    //  //printfn "%s Registered" clientId
                    //  let registerRetObj = {recordDefaultRegistration with Message = "Registered"}
                    //  clientObject <! registerRetObj
               | _-> ()     
        | :? addSubscribedUsers as msg -> //Subscribing a user to another user
               let message = msg.Message
               
               match message with
               | "Subscribe" ->
                    printfn "Coming %A" msg
                    subscribe(msg)
                    //  let clientId = msg.ClientName
                    //  let subscriberName = msg.SubscriberName
                    //  let key,actorSelectObject = userDictionary.TryGetValue clientId
                    //  let client,subscriberList = subscriberDictionary.TryGetValue clientId
                    //  if not(subscriberList.Contains(subscriberName)) then //add the user to the subscriber list if he is not there
                    //    subscriberList.Add(subscriberName)
                    //  let subscriberRetObj = {msg with Message = "Subscribed"} 
                    //  //printfn "%s subscribed to %s" subscriberName clientId
                    //  actorSelectObject <! subscriberRetObj 
               | _-> ()
        | :? recordTweets as msg -> //Find the type of tweet if its normal tweet or mention
             recordTweets(msg)
            //    let tweet = msg.Tweet
            //    let clientTweeting = msg.ClientName 
            //    let words = tweet.Split [|' '|]
            //    let mutable messageType = "NormalTweet"
            //    let clientsMentionedList = new List<string>()
               
            //    for word in words do
            //    //if it's a mention then add the mentioned users to the list
            //       if Regex.IsMatch(word, "^(@)([a-zA-Z])+") then
            //          clientsMentionedList.Add(word.Remove(0,1))
            //          messageType <- "Mentions"
            //      //if it's a hashtag then add the hashtag and the tweet to the hashTagsDict and hashTagsUserDict    
            //       else if Regex.IsMatch(word, "^(#)([a-zA-Z0-9])+") then
            //          let hashTag = word
            //          if not(hashtagDictionary.ContainsKey(hashTag)) then
            //             hashtagDictionary.Add(hashTag,new List<string>())
            //          let htkey,hashTagList = hashtagDictionary.TryGetValue hashTag
            //          hashTagList.Add(tweet)
            //          let htUserKey,hashTagUserList = userHashtagDictionary.TryGetValue clientTweeting
            //          hashTagUserList.Add(tweet)

            //    //add this tweet to the user's tweetList in tweetsDictionary                           
            //    let key,tweetsList = tweetsDictionary.TryGetValue clientTweeting
            //    tweetsList.Add(tweet)  

            //    match messageType with
            //    | "NormalTweet" -> 
            //  //send the tweet to all the subscribers and add this tweet to the list of tweets the subscribers have subscribed to
            //          let key1,subscriberList =  subscriberDictionary.TryGetValue clientTweeting
            //          for subscriber in subscriberList do
            //             let tweetToBeSent = {msg with Message = "GetTweets"}
            //             let key2,actorSelectObject = userDictionary.TryGetValue subscriber
            //             let key3,subscribedToTweetsList = subscribedToTweetsDictionary.TryGetValue subscriber 
            //             subscribedToTweetsList.Add(tweet)
            //            // //printfn "%A" subscribedToTweetsList
            //             actorSelectObject <! tweetToBeSent
            //             //printfn "%s's Tweet \"%s\" sent to %s"  clientTweeting tweet subscriber
            //     | "Mentions" -> //send tweets to only the mentioned users
            //          for mentionedClient in  clientsMentionedList do
            //             let mentionKey,mentionedList = mentionDictionary.TryGetValue mentionedClient
            //             let key2,actorSelectObject = userDictionary.TryGetValue mentionedClient
            //             mentionedList.Add(tweet)
            //             let sendMentionTweet = {msg with Message = "GetMentionedTweets"}
            //             actorSelectObject <! sendMentionTweet
            //             //printfn "%s has been mentioned in Tweet \"%s\" of %s. Thus this tweet sent to %s"  mentionedClient tweet clientTweeting mentionedClient

            //   | _->()

        | :? recordRetweets as msg ->
               recordRetweet(msg)
              //  let retweetingClient = msg.ClientName
              //  let random = System.Random()
              //  let key,subscribedTweetsList = subscribedToTweetsDictionary.TryGetValue retweetingClient
              //  let subscribedTweetsArray = subscribedTweetsList.ToArray()
              //  let length =  subscribedTweetsArray.Length
              //  if length > 0 then
              //    let randomIndex = random.Next(0,length)
              //    let mutable tweet = subscribedTweetsArray.[randomIndex] 
              //    tweet <- "Retweet by " + retweetingClient + " :: " + tweet
              //    let recordTweets = {ClientName = retweetingClient;Message = "Tweets"; Tweet=tweet}
              //    let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
              //    serverEngine <! recordTweets
        | :? recordQuery as msg ->
                  recordQuery(msg)
              //  let message = msg.Message
              //  let clientName = msg.ClientName
              //  let key2,actorSelectObject = userDictionary.TryGetValue clientName
              //  match message with
              //  | "ReturnAllSubscribedTweets" ->      
              //       let key,subscribedToTweetsList = subscribedToTweetsDictionary.TryGetValue clientName
              //       let sendsubscribedToList = subscribedToTweetsList
              //       let recordQuery = {msg with TweetsList = sendsubscribedToList; Message = "GetTweetsSubscribedTo"}
              //       actorSelectObject <! recordQuery
              //  | "ReturnAllTweetsWithHashtags" ->
              //       let hashtag = msg.HashTag
              //       let key,hashtagsList = hashtagDictionary.TryGetValue hashtag
              //       let recordQuery = {msg with TweetsList = hashtagsList; Message = "GetTweetsWithHashTags"}
              //       actorSelectObject <! recordQuery
              //  | "ReturnTweetsWithMentions" ->
              //       let key,mentionedList = mentionDictionary.TryGetValue clientName
              //       let recordQuery = {msg with TweetsList = mentionedList; Message = "GetMentionedTweets"}
              //       actorSelectObject <! recordQuery      
              //  | _-> ()     

        | _-> ()
     
   
    
