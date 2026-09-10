using System;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Runtime.Singleton;
using UnityEngine;
#if BMH_NOTIFICATION
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
#endif

[Singleton("NotificationManager", true)]
public class NotificationManager : Singleton<NotificationManager>
{
//    <meta-data
//        android:name="com.google.firebase.messaging.default_notification_icon"
//    android:resource="@drawable/firebase_notification_icon" />

//sound name
// notification.waw
    public override void Init()
    {
    }

#if BMH_NOTIFICATION

    private void Start()
    {
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Debug.Log("Firebase Message");
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
    }
#endif
}