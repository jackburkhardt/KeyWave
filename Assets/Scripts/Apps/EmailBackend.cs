using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Yarn.Unity;

namespace Apps
{
    public class EmailBackend : ScriptableObject
    {
        private static string emailsPath;
        private static string inboxPath;
        private static List<Email> playerInbox = new List<Email>();

        public void Awake()
        {
            emailsPath = Application.streamingAssetsPath + "/GameData/Emails/";
            inboxPath = Application.streamingAssetsPath + "/GameData/Emails/inbox.json";

            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
        }

        /// <summary>
        /// "Delivers" an email by marking it as available and adding it to the player's inbox.
        /// </summary>
        [YarnCommand("deliver_email")]
        public static void DeliverEmail(string subject)
        {
            Email email = playerInbox.Find(e => e.Subject == subject);
            if (email.Equals(default)) return;
            
            GameEvent.DeliverEmail(email);
        }
        
        public static List<Texture2D> LoadEmailImages(string[] paths)
        {
            List<Texture2D> loadedImages = new List<Texture2D>();

            foreach (var path in paths)
            {
                if (!File.Exists(emailsPath + path)) continue;

                var fileData = File.ReadAllBytes(emailsPath + path);
                var tex = new Texture2D(2, 2); // Create new "empty" texture
                if (tex.LoadImage(fileData))
                {
                    loadedImages.Add(tex);
                }
            }

            return loadedImages;
        }
        
        private void Save() => DataManager.SerializeData(playerInbox, inboxPath);
        private void Load() => playerInbox = DataManager.DeserializeData<List<Email>>(inboxPath);

        public static List<Email> PlayerInbox => playerInbox;
        
        public struct Email
        {
            public string Sender;
            public string Recipient;
            public string Subject;
            public string BodyText;
            public string[] BodyImagePaths;
            public bool Read;
            public bool Available;

            public Email(string sender, string recipient, string subject, string bodyText, 
                string[] bodyImagePaths, bool read = false, bool available = false)
            {
                Sender = sender;
                Recipient = recipient;
                Subject = subject;
                BodyText = bodyText;
                BodyImagePaths = bodyImagePaths;
                Read = read;
                Available = available;
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnGameSave -= Save;
            GameEvent.OnGameLoad -= Load;
        }
    }
}