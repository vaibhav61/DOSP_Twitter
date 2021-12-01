#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
#r "nuget: Akka.Remote"

open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Collections.Generic

type recordRegistration = {
    Client:int;
    Message: string;
    SubscriberCount:int;
    ClientID:string;
    ActorObj:ActorSelection
}

type addSubscribedUsers = {
    ClientName:string;
    SubscriberName:string;
    Message: string
}

type recordTweets = {
    ClientName:string;
    Message: string;
    Tweet: string; 
}

type recordRetweets = {
    ClientName:string;
    Message: string;
}

type recordPrint = {
    Client:string;
    Message:string
}

type recordQuery = {
    ClientName:string;
    Message:string;
    HashTag:string
    TweetsList:List<string>
}

let recordDefaultRegistration = {Client=0; Message = "default"; SubscriberCount = -1; ClientID = "default"; ActorObj = null}
let addDefaultSubscribedUsers = {ClientName="-1"; SubscriberName="-1"; Message = "default"}
let recordDefaultTweet = {ClientName = "-1"; Message="default";Tweet="-1"}
let recordDefaultRetweet = {ClientName = "-1"; Message = "default"}
let recordDefaultPrint = {Client = ""; Message = ""}
let recordDefaultQuery = {ClientName = ""; HashTag = ""; Message = "";TweetsList = null}