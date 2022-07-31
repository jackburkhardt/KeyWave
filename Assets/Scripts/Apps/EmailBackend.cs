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
            emailsPath = Application.dataPath + "/GameData/Emails/";
            inboxPath = Application.dataPath + "/GameData/Emails/inbox.json";

            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
        }

        [YarnCommand("deliver_email")]
        public static void DeliverEmail(string subject)
        {
            Email email = LoadEmailFromDisk(subject);
            playerInbox.Add(email);
            GameEvent.DeliverEmail(email);
        }

        private static Email LoadEmailFromDisk(string subject)
        {
            try
            {
                Email email = JsonConvert.DeserializeObject<Email>(File.ReadAllText(emailsPath + subject + ".json"));
                return email;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

            public Email(string sender, string recipient, string subject, string bodyText, string[] bodyImagePaths)
            {
                Sender = sender;
                Recipient = recipient;
                Subject = subject;
                BodyText = bodyText;
                BodyImagePaths = bodyImagePaths;
                Read = false;
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnGameSave -= Save;
            GameEvent.OnGameLoad -= Load;
        }
    }
}