#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
#r "nuget: Akka.Remote"
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


let configuration = 
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
                hostname = ""0.0.0.0""
                port = 1000
            }
        }")

let system = ActorSystem.Create ("Twitter", configuration)
let userDictionary = new Dictionary<string, ActorSelection>()
let clientsList = new List<string>()
let noOfSubscribersDictionary = new Dictionary<string,int>()
let mutable totalActors = 0 
//let timer = Stopwatch()
let randomWords = [|"aback";"abaft";"abandoned";"abashed";"aberrant";"abhorrent";"abiding";"abject";"ablaze";"able";"abnormal";"aboard";"aboriginal";"abortive";"abounding";"abrasive";"abrupt";"absent";"absorbed";"absorbing";"abstracted";"absurd";"abundant";"abusive";"accept";"acceptable";"accessible";"accidental";"account";"accurate";"achiever";"acid";"acidic";"acoustic";"acoustics";"acrid";"act";"action";"activity";"actor";"actually";"ad hoc";"adamant";"adaptable";"add";"addicted";"addition";"adhesive";"adjoining";"adjustment";"admire";"admit";"adorable";"adventurous";"advertisement";"advice";"advise";"afford";"afraid";"aftermath";"afternoon";"afterthought";"aggressive";"agonizing";"agree";"agreeable";"agreement";"ahead";"air";"airplane";"airport";"ajar";"alarm";"alcoholic";"alert";"alike";"alive";"alleged";"allow";"alluring";"aloof";"amazing";"ambiguous";"ambitious";"amount";"amuck";"amuse";"amused";"amusement";"amusing";"analyze";"ancient";"anger";"angle";"angry";"animal";"animated";"announce";"annoy";"annoyed";"annoying";"answer";"ants";"anxious";"apathetic";"apologise";"apparatus";"apparel";"appear";"applaud";"appliance";"appreciate";"approval";"approve";"aquatic";"arch";"argue";"argument";"arithmetic";"arm";"army";"aromatic";"arrange";"arrest";"arrive";"arrogant";"art";"ashamed";"ask";"aspiring";"assorted";"astonishing";"attach";"attack";"attempt";"attend";"attract";"attraction";"attractive";"aunt";"auspicious";"authority";"automatic";"available";"average";"avoid";"awake";"aware";"awesome";"awful";"axiomatic";"babies";"baby";"back";"bad";"badge";"bag";"bait";"bake";"balance";"ball";"ban";"bang";"barbarous";"bare";"base";"baseball";"bashful";"basin";"basket";"basketball";"bat";"bath";"bathe";"battle";"bawdy";"bead";"beam";"bear";"beautiful";"bed";"bedroom";"beds";"bee";"beef";"befitting";"beg";"beginner";"behave";"behavior";"belief";"believe";"bell";"belligerent";"bells";"belong";"beneficial";"bent";"berry";"berserk";"best";"better";"bewildered";"big";"bike";"bikes";"billowy";"bird";"birds";"birth";"birthday";"bit";"bite";"bite-sized";"bitter";"bizarre";"black";"black-and-white";"blade";"bleach";"bless";"blind";"blink";"blood";"bloody";"blot";"blow";"blue";"blue-eyed";"blush";"blushing";"board";"boast";"boat";"boil";"boiling";"bolt";"bomb";"bone";"book";"books";"boorish";"boot";"border";"bore";"bored";"boring";"borrow";"bottle";"bounce";"bouncy";"boundary";"boundless";"bow";"box";"boy";"brainy";"brake";"branch";"brash";"brass";"brave";"brawny";"breakable";"breath";"breathe";"breezy";"brick";"bridge";"brief";"bright";"broad";"broken";"brother";"brown";"bruise";"brush";"bubble";"bucket";"building";"bulb";"bump";"bumpy";"burly";"burn";"burst";"bury";"bushes";"business";"bustling";"busy";"butter";"button";"buzz";"cabbage";"cable";"cactus";"cagey";"cake";"cakes";"calculate";"calculating";"calculator";"calendar";"call";"callous";"calm";"camera";"camp";"can";"cannon";"canvas";"cap";"capable";"capricious";"caption";"car";"card";"care";"careful";"careless";"caring";"carpenter";"carriage";"carry";"cars";"cart";"carve";"cast";"cat";"cats";"cattle";"cause";"cautious";"cave";"ceaseless";"celery";"cellar";"cemetery";"cent";"certain";"chalk";"challenge";"chance";"change";"changeable";"channel";"charge";"charming";"chase";"cheap";"cheat";"check";"cheer";"cheerful";"cheese";"chemical";"cherries";"cherry";"chess";"chew";"chicken";"chickens";"chief";"childlike";"children";"chilly";"chin";"chivalrous";"choke";"chop";"chubby";"chunky";"church";"circle";"claim";"clam";"clammy";"clap";"class";"classy";"clean";"clear";"clever";"clip";"cloistered";"close";"closed";"cloth";"cloudy";"clover";"club";"clumsy";"cluttered";"coach";"coal";"coast";"coat";"cobweb";"coherent";"coil";"cold";"collar";"collect";"color";"colorful";"colossal";"colour";"comb";"combative";"comfortable";"command";"committee";"common";"communicate";"company";"compare";"comparison";"compete";"competition";"complain";"complete";"complex";"concentrate";"concern";"concerned";"condemned";"condition";"confess";"confuse";"confused";"connect";"connection";"conscious";"consider";"consist";"contain";"continue";"control";"cooing";"cook";"cool";"cooperative";"coordinated";"copper";"copy";"corn";"correct";"cough";"count";"country";"courageous";"cover";"cow";"cowardly";"cows";"crabby";"crack";"cracker";"crash";"crate";"craven";"crawl";"crayon";"crazy";"cream";"creator";"creature";"credit";"creepy";"crib";"crime";"crook";"crooked";"cross";"crow";"crowd";"crowded";"crown";"cruel";"crush";"cry";"cub";"cuddly";"cultured";"cumbersome";"cup";"cure";"curious";"curl";"curly";"current";"curtain";"curve";"curved";"curvy";"cushion";"cut";"cute";"cycle";"cynical";"dad";"daffy";"daily";"dam";"damage";"damaged";"damaging";"damp";"dance";"dangerous";"dapper";"dare";"dark";"dashing";"daughter";"day";"dazzling";"dead";"deadpan";"deafening";"dear";"death";"debonair";"debt";"decay";"deceive";"decide";"decision";"decisive";"decorate";"decorous";"deep";"deeply";"deer";"defeated";"defective";"defiant";"degree";"delay";"delicate";"delicious";"delight";"delightful";"delirious";"deliver";"demonic";"depend";"dependent";"depressed";"deranged";"describe";"descriptive";"desert";"deserted";"deserve";"design";"desire";"desk";"destroy";"destruction";"detail";"detailed";"detect";"determined";"develop";"development";"devilish";"didactic";"different";"difficult";"digestion";"diligent";"dime";"dinner";"dinosaurs";"direction";"direful";"dirt";"dirty";"disagree";"disagreeable";"disappear";"disapprove";"disarm";"disastrous";"discover";"discovery";"discreet";"discussion";"disgusted";"disgusting";"disillusioned";"dislike";"dispensable";"distance";"distinct";"distribution";"disturbed";"divergent";"divide";"division";"dizzy";"dock";"doctor";"dog";"dogs";"doll";"dolls";"domineering";"donkey";"door";"double";"doubt";"doubtful";"downtown";"drab";"draconian";"drag";"drain";"dramatic";"drawer";"dream";"dreary";"dress";"drink";"drip";"driving";"drop";"drown";"drum";"drunk";"dry";"duck";"ducks";"dull";"dust";"dusty";"dynamic";"dysfunctional";"eager";"ear";"early";"earn";"earsplitting";"earth";"earthquake";"earthy";"easy";"eatable";"economic";"edge";"educate";"educated";"education";"effect";"efficacious";"efficient";"egg";"eggnog";"eggs";"eight";"elastic";"elated";"elbow";"elderly";"electric";"elegant";"elfin";"elite";"embarrass";"embarrassed";"eminent";"employ";"empty";"enchanted";"enchanting";"encourage";"encouraging";"end";"endurable";"energetic";"engine";"enjoy";"enormous";"enter";"entertain";"entertaining";"enthusiastic";"envious";"equable";"equal";"erect";"erratic";"error";"escape";"ethereal";"evanescent";"evasive";"even";"event";"examine";"example";"excellent";"exchange";"excite";"excited";"exciting";"exclusive";"excuse";"exercise";"exist";"existence";"exotic";"expand";"expansion";"expect";"expensive";"experience";"expert";"explain";"explode";"extend";"extra-large";"extra-small";"exuberant";"exultant";"eye";"eyes";"fabulous";"face";"fact";"fade";"faded";"fail";"faint";"fair";"fairies";"faithful";"fall";"fallacious";"false";"familiar";"famous";"fanatical";"fancy";"fang";"fantastic";"far";"far-flung";"farm";"fascinated";"fast";"fasten";"fat";"faulty";"fax";"fear";"fearful";"fearless";"feeble";"feeling";"feigned";"female";"fence";"fertile";"festive";"fetch";"few";"field";"fierce";"file";"fill";"film";"filthy";"fine";"finger";"finicky";"fire";"fireman";"first";"fish";"fit";"five";"fix";"fixed";"flag";"flagrant";"flaky";"flame";"flap";"flash";"flashy";"flat";"flavor";"flawless";"flesh";"flight";"flimsy";"flippant";"float";"flock";"flood";"floor";"flow";"flower";"flowers";"flowery";"fluffy";"fluttering";"fly";"foamy";"fog";"fold";"follow";"food";"fool";"foolish";"foot";"force";"foregoing";"forgetful";"fork";"form";"fortunate";"found";"four";"fowl";"fragile";"frail";"frame";"frantic";"free";"freezing";"frequent";"fresh";"fretful";"friction";"friend";"friendly";"friends";"frighten";"frightened";"frightening";"frog";"frogs";"front";"fruit";"fry";"fuel";"full";"fumbling";"functional";"funny";"furniture";"furry";"furtive";"future";"futuristic";"fuzzy";"gabby";"gainful";"gamy";"gaping";"garrulous";"gate";"gather";"gaudy";"gaze";"geese";"general";"gentle";"ghost";"giant";"giants";"giddy";"gifted";"gigantic";"giraffe";"girl";"girls";"glamorous";"glass";"gleaming";"glib";"glistening";"glorious";"glossy";"glove";"glow";"glue";"godly";"gold";"good";"goofy";"gorgeous";"government";"governor";"grab";"graceful";"grade";"grain";"grandfather";"grandiose";"grandmother";"grape";"grass";"grate";"grateful";"gratis";"gray";"grease";"greasy";"great";"greedy";"green";"greet";"grey";"grieving";"grin";"grip";"groan";"groovy";"grotesque";"grouchy";"ground";"group";"growth";"grubby";"gruesome";"grumpy";"guarantee";"guard";"guarded";"guess";"guide";"guiltless";"guitar";"gullible";"gun";"gusty";"guttural";"habitual";"hair";"haircut";"half";"hall";"hallowed";"halting";"hammer";"hand";"handle";"hands";"handsome";"handsomely";"handy";"hang";"hanging";"hapless";"happen";"happy";"harass";"harbor";"hard";"hard-to-find";"harm";"harmonious";"harmony";"harsh";"hat";"hate";"hateful";"haunt";"head";"heady";"heal";"health";"healthy";"heap";"heartbreaking";"heat";"heavenly";"heavy";"hellish";"help";"helpful";"helpless";"hesitant";"hideous";"high";"high-pitched";"highfalutin";"hilarious";"hill";"hissing";"historical";"history";"hobbies";"hole";"holiday";"holistic";"hollow";"home";"homeless";"homely";"honey";"honorable";"hook";"hop";"hope";"horn";"horrible";"horse";"horses";"hose";"hospitable";"hospital";"hot";"hour";"house";"houses";"hover";"hug";"huge";"hulking";"hum";"humdrum";"humor";"humorous";"hungry";"hunt";"hurried";"hurry";"hurt";"hushed";"husky";"hydrant";"hypnotic";"hysterical";"ice";"icicle";"icky";"icy";"idea";"identify";"idiotic";"ignorant";"ignore";"ill";"ill-fated";"ill-informed";"illegal";"illustrious";"imaginary";"imagine";"immense";"imminent";"impartial";"imperfect";"impolite";"important";"imported";"impossible";"impress";"improve";|]
let mutable currentSubCount = 0
let mutable subscriberCount = 0
let mutable clientIndex = 0
let mutable clientId = ""
let mutable tweets = 0 
let mutable actorSelectObject: ActorSelection = null
let getWords() = 
    let random  = System.Random()
    let randomIndex = random.Next(0,randomWords |> Array.length ) 
    let randowmIndex2 = random.Next(0,randomWords |> Array.length )
    let mutable res = ""
    if randomIndex<= randowmIndex2 then
     for i in [0 .. randomIndex] do
         if(res.Length<144) then
          res <- res + randomWords.[i] + " "
     else 
     for i in [0 .. randomIndex] do
         if(res.Length<144) then
          res <- res + randomWords.[i] + " "
    res  
type Print () = 
  inherit Actor()

  override x.OnReceive(msg) = 
        match msg with 
        | :? recordPrint as msg-> 
            let clientName = msg.Client
            let printMessage = msg.Message 
            if clientName <> "-1" then
                printfn "%s Homepage :" clientName
            printfn "%s" printMessage
        | _->()      

let printactorRef = system.ActorOf(Props.Create(typeof<Print>), "printactor")

let inintializeClient(msg: recordRegistration) = 
                let objectRegister = msg
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                clientIndex <- objectRegister.Client
                subscriberCount <- objectRegister.SubscriberCount
                actorSelectObject <- objectRegister.ActorObj
                clientId <- objectRegister.ClientID
                tweets <- objectRegister.SubscriberCount
                serverEngine <! objectRegister
let registered(msg: recordRegistration) =
                let printMessge = clientId + " has registered with the server"
                let printRecord = {Client = clientId; Message = printMessge}
                printactorRef <! printRecord   

let subscribe(msg: addSubscribedUsers) = 
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                let mutable i = 0
                let mutable startIndex = clientIndex - subscriberCount
                if startIndex < 0 then
                    startIndex <- 0 
                
                for i in [0 .. subscriberCount-1] do
                    if startIndex = clientIndex then
                       startIndex <- startIndex + 1

                    let subscriberId = "client"+string(startIndex+1)
                    let subscriberObject = {ClientName = clientId; SubscriberName = subscriberId;Message = "Subscribe"}
                    startIndex <- startIndex + 1
                    serverEngine <! subscriberObject

let subscribed(msg: addSubscribedUsers) = 
                let clientName = msg.ClientName
                let subscriberName = msg.SubscriberName
                let printMessge = subscriberName+" subscribed to "+clientName
                let printRecord = {Client = clientName; Message = printMessge}
                printactorRef <! printRecord

let TweetsCall(i,n) =
            let random  = System.Random()
            let ranIndex = random.Next(1,3)
            if(ranIndex = 1) then
            //simulate Normal Tweets
                let clientName, actorSelection = userDictionary.TryGetValue ("client"+string(i+1))
                let noOfSubscribers = subscriberCount;
                let tweetRecord = {recordDefaultTweet with Message = "Tweets"}
                actorSelection <! tweetRecord
                Thread.Sleep(10)

            Thread.Sleep(n*40)
            printfn "Simulation of tweets with hashtags"
            if(ranIndex = 2) then
            //simulate tweets with hashtags
                let clientName, actorSelection = userDictionary.TryGetValue ("client"+string(i+1))
                let tweetRecord = {recordDefaultTweet with Message = "TweetsWithHashTags"}
                actorSelection <! tweetRecord
                Thread.Sleep(10)

            Thread.Sleep(n*50)
            printfn "Simulation of tweets with mentions"
            if(ranIndex = 3) then
            //simulate tweets with mentions
                let clientName, actorSelection = userDictionary.TryGetValue ("client"+string(i+1))
                let tweetRecord = {recordDefaultTweet with Message = "TweetsWithMentions"}
                actorSelection <! tweetRecord
                Thread.Sleep(10)


type Client () =
  inherit Actor() 
  
  override x.OnReceive(msg) =   
      
        match msg with  
        | :? recordRegistration as msg -> 
            let message = msg.Message
            match message with 
            | "Initialize Client" ->
                let objectRegister = msg
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                clientIndex <- objectRegister.Client
                subscriberCount <- objectRegister.SubscriberCount
                actorSelectObject <- objectRegister.ActorObj
                clientId <- objectRegister.ClientID
                tweets <- objectRegister.SubscriberCount
                serverEngine <! objectRegister
            | "Registered" ->
                let printMessge = clientId + " has registered with the server"
                let printRecord = {Client = clientId; Message = printMessge}
                printactorRef <! printRecord          
            | _-> ()    
        | :? addSubscribedUsers as msg ->
             let message = msg.Message
             match message with 
             | "Subscribe"  ->  
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                let mutable i = 0
                let mutable startIndex = clientIndex - subscriberCount
                if startIndex < 0 then
                    startIndex <- 0 
                
                for i in [0 .. subscriberCount-1] do
                    if startIndex = clientIndex then
                       startIndex <- startIndex + 1

                    let subscriberId = "client"+string(startIndex+1)
                    printfn "subscriberId %s %s " subscriberId, subscriberCount 
                    let subscriberObject = {ClientName = clientId; SubscriberName = subscriberId;Message = "Subscribe"}
                    startIndex <- startIndex + 1
                    serverEngine <! subscriberObject  
             | "Subscribed" -> 
                let clientName = msg.ClientName
                let subscriberName = msg.SubscriberName
                let printMessge = subscriberName+" subscribed to "+clientName
                let printRecord = {Client = clientName; Message = printMessge}
                printactorRef <! printRecord  
             | _-> ()
        | :? recordTweets as msg -> 
             let message = msg.Message
             match message with 
             | "Tweets" -> //send tweets to all my subscribers
                let tweetInfo = getWords() + clientId+ getWords()
                let tweetRecord = {ClientName = clientId; Message ="Tweets";Tweet = tweetInfo}
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                serverEngine <! tweetRecord
             | "TweetsWithHashTags" -> //send tweets with hashtags to all my subscribers
                let tweetInfo = "#FootballWorldCup "+clientId+" tweet by "+clientId
                let tweetRecord = {ClientName = clientId; Message ="SendTweetsWithHashtags";Tweet = tweetInfo}
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                serverEngine <! tweetRecord
             | "TweetsWithMentions" -> //send tweets with mentions to all the users mentioned
                let random  = System.Random()
                let mentionedSubscribers = random.Next(1,4)
                let userArray = clientsList.ToArray()
                let length = userArray.Length
                let mentionedSubscriberList = new List<string>() 
                
                for i in [0 .. mentionedSubscribers-1] do
                  let randomIndex = random.Next(0,length)
                  let user = userArray.[randomIndex]
                  let userSet = Set.ofSeq mentionedSubscriberList
                  if (user <> clientId && not(userSet.Contains(user))) then
                     mentionedSubscriberList.Add(user)
                
                //generate the tweet message by mentioning each of the random user selected to be mentioned
                let mutable tweetInfo = "Hi "+clientId+" mentioning"
                for user in mentionedSubscriberList do
                    tweetInfo <- tweetInfo + " @"+user

                //send the message to the server so as to send the tweet to the mentioned users
                let tweetRecord = {ClientName = clientId; Message = "SendTweetsWithMentions";Tweet = tweetInfo}
                let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
                serverEngine <! tweetRecord    

             | "GetTweets" -> //receive tweets from everyone to whom I am subscribed to
                let sender = msg.ClientName
                let tweetMessageReceived = msg.Tweet
                let printMessge = clientId+" received the tweet \""+tweetMessageReceived+ "\" from "+sender
                let printRecord = {Client = clientId; Message = printMessge}
                printactorRef <! printRecord 

             | "GetMentionedTweets" -> //receive tweets where u have been mentioned at
                let sender = msg.ClientName
                let tweetMessageReceived = msg.Tweet
                let printMessge = sender+" mentioned "+clientId+ " in his tweet \""+tweetMessageReceived+"\""
                let printRecord = {Client = clientId; Message = printMessge}
                printactorRef <! printRecord 

             |_-> ()

        | :? recordRetweets as msg -> //retweet 
             let recordRetweet = {ClientName = clientId; Message="ReTweet"}
             let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
             serverEngine <! recordRetweet
        | :? recordQuery as msg ->
             let message = msg.Message
             let serverEngine = system.ActorSelection("akka.tcp://Twitter@localhost:8000/user/server1")
             match message with 
             | "ReturnAllSubscribedTweets" ->
                let queryRcd = {ClientName = clientId;Message="ReturnAllSubscribedTweets";HashTag = "";TweetsList=null} 
                serverEngine <! queryRcd
             | "ReturnAllTweetsWithHashtags" ->
                let queryRcd = {ClientName = clientId;Message="ReturnAllTweetsWithHashtags";HashTag="#DOSisAmazing";TweetsList=null}
                serverEngine <! queryRcd
             | "ReturnTweetsWithMentions" ->
                let queryRcd = {ClientName = clientId;Message="ReturnTweetsWithMentions";HashTag="";TweetsList=null}
                serverEngine <! queryRcd 
             | "GetTweetsSubscribedTo" ->
                let listOfTweets = msg.TweetsList
                if listOfTweets.Count > 0 then 
                    let printRecordObj = {Client = "\n\n"+clientId;Message = "QueryPerformance \nAll Subscribed Tweets"}
                    printactorRef <! printRecordObj
                    for tweet in listOfTweets do
                       let printRecordObj1 = {Client = "-1";Message = tweet}
                       printactorRef <! printRecordObj1
             | "GetTweetsWithHashTags" -> 
                let hashtagTweetsList = msg.TweetsList
                let hashTag = msg.HashTag
                if hashtagTweetsList.Count > 0 then
                    let printRecordObj = {Client = "-1";Message = "\n\nAll Tweets with "+hashTag}
                    printactorRef <! printRecordObj
                    for tweet in hashtagTweetsList do
                       let printRecordObj1 = {Client = "-1";Message = tweet}
                       printactorRef <! printRecordObj1
             | "GetMentionedTweets" ->
                let mentionedTweetsList = msg.TweetsList
                if mentionedTweetsList.Count > 0 then
                    let printRecordObj = {Client = "-1";Message = "\n\nAll Tweets where "+clientId+" has been mentioned"}
                    printactorRef <! printRecordObj
                    for tweet in mentionedTweetsList do
                       let printRecordObj1 = {Client = "-1";Message = tweet}
                       printactorRef <! printRecordObj1                  
             | _-> () 
        |_-> () 
             let combined = (string)msg
             let i = combined.Split("_").[0]
             let n = combined.Split("_").[1]
             let random  = System.Random()
             let ranIndex = random.Next(1,3)
             if(ranIndex = 1) then
             //simulate Normal Tweets
                let clientName, actorSelection = userDictionary.TryGetValue ("client"+string((int)i+1))
                let noOfSubscribers = subscriberCount;
                let tweetRecord = {recordDefaultTweet with Message = "Tweets"}
                actorSelection <! tweetRecord
                Thread.Sleep(10)

             Thread.Sleep((int)n * 40)
             printfn "Simulation of tweets with hashtags"
             if(ranIndex = 2) then
             //simulate tweets with hashtags
                let clientName, actorSelection = userDictionary.TryGetValue ("client"+string((int)i+1))
                let tweetRecord = {recordDefaultTweet with Message = "TweetsWithHashTags"}
                actorSelection <! tweetRecord
                Thread.Sleep(10)

             Thread.Sleep((int)n*50)
             printfn "Simulation of tweets with mentions"
             if(ranIndex = 3) then
             //simulate tweets with mentions
                let clientName, actorSelection = userDictionary.TryGetValue ("client"+string((int)i+1))
                let tweetRecord = {recordDefaultTweet with Message = "TweetsWithMentions"}
                actorSelection <! tweetRecord
                Thread.Sleep(10)
               

let init param = 
    let n, maxSubscriberCount = param
    let prevSubCount = maxSubscriberCount;
    totalActors <- n
    let random  = System.Random()
    
//spawn client actors 
    for i in [0 .. n-1] do
       // printfn "string %i" (i+1)
        let clientId = "client"+string(i+1)
        clientsList.Add(clientId)
        system.ActorOf(Props.Create(typeof<Client>), clientId)|>ignore
        let actorSelect = "akka.tcp://Twitter@0.0.0.0:1000/user/"+clientId
        let client1 = system.ActorSelection(actorSelect)
        userDictionary.Add(clientId,client1)

    Thread.Sleep(30)
    printfn "Client registration begin"
  //set subscribercnt and name for each client and register each client with server 

    for i in [0 .. n-1] do
        let clientId = "client"+string(i+1)
        let clientName,actorSelection = userDictionary.TryGetValue clientId
        let subsciberCnt = prevSubCount
        noOfSubscribersDictionary.TryAdd(clientId,subsciberCnt) |>ignore
        prevSubCount = prevSubCount/(i+1) |>ignore
        let clientRegister = {Client = i; Message = "Initialize Client";SubscriberCount = subsciberCnt;ClientID = clientId;ActorObj = actorSelection}
        actorSelection <! clientRegister
        Thread.Sleep(10)
    
    Thread.Sleep(n*30)
    printfn "Simulation of Client Subscription"
//set subscribers for each client    
    for i in [0 .. n-1] do
        let clientName, actorSelection = userDictionary.TryGetValue ("client"+string(i+1)) 
        let defaultSubscribeRegister = {addDefaultSubscribedUsers with Message = "Subscribe"}
        actorSelection <! defaultSubscribeRegister
        Thread.Sleep(10)
    
    Thread.Sleep(n*30)
    printfn "Simulation of tweets"
    let mutable tweetFrequency = n+1
    for i in [0 .. n-1] do
        let randomIndex = random.Next(0,n)
        let b,k=noOfSubscribersDictionary.TryGetValue("client"+string(randomIndex))
        for j = 0 to k do 
            TweetsCall(i,n)
        

    Thread.Sleep(n*60)
    printfn "Simulation of retweets"
    for i in [0 .. n-1] do
        let clientName, actorSelection = userDictionary.TryGetValue ("client"+string(i+1))
        let retweetRecord = {recordDefaultRetweet with Message = "ReTweet"}
        actorSelection <! retweetRecord
        Thread.Sleep(10)
    
    Thread.Sleep(n*60)
    let connectedList = new List<string>()
    let connectedUsers = n/2;
    let mutable j = 0
    let userArray = clientsList.ToArray()

    while j < connectedUsers do
        let randomIndex = random.Next(0,n)
        let user = userArray.[randomIndex]
        let userSet = Set.ofSeq connectedList
        if not(userSet.Contains(user)) then
            connectedList.Add(user)
            j <- j+1

    Thread.Sleep(n*80)        
    for i in [0..n-1] do
        let user = userArray.[i]
        if not(connectedList.Contains(user)) then
            let msg = user + " has disconnected"
            let recordPrint = {Client = user;Message = msg}
            printactorRef <! recordPrint

    Thread.Sleep(n*120)
    for user in connectedList do
        let clientName, actorSelection = userDictionary.TryGetValue user 
        let receivedQuery1 = {recordDefaultQuery with Message = "ReturnAllSubscribedTweets"}
        actorSelection <! receivedQuery1

        let receivedQuery2 = {recordDefaultQuery with Message = "ReturnAllTweetsWithHashtags"}
        actorSelection <! receivedQuery2

        let receivedQuery3 = {recordDefaultQuery with Message = "ReturnTweetsWithMentions"}
        actorSelection <! receivedQuery3   
        
    
    
    
let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numberOfActors = 10
//args.[0] |> int
let mutable maxSubCount = 10
if numberOfActors <= 5 then
   maxSubCount <- numberOfActors-1
let mutable startTime = 0
init(numberOfActors,maxSubCount) 
let endTime  = System.DateTime.Now.TimeOfDay.Milliseconds
let timeTaken = abs(endTime - startTime)
printfn "time taken : %i" +timeTaken

System.Console.ReadLine() |> ignore
system.Terminate() |> ignore
