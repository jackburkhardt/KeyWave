using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using File = System.IO.File;

namespace Apps
{
    public class EmailManager : ScriptableObject
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
            TestEmail();
        }

        private void TestEmail()
        {
            Email email = new Email("Steve Becker", "Ava", "An idea for managing finances",
                "Hi Ava, \n\ntake a look at these pictures and" +
                "let me know what you think. \n\nThis could be big.",
                new string[] { "idea-image1.png", "idea-image2.jpg" });
            
            StreamWriter sw = !File.Exists(emailsPath + email.Subject + ".json") ? File.CreateText(emailsPath + email.Subject + ".json") 
                : new StreamWriter(emailsPath + email.Subject + ".json");
            string json = JsonConvert.SerializeObject(email, Formatting.Indented);
            sw.Write(json);
            sw.Close();
            playerInbox.Add(email);
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

        private void Save()
        {
            StreamWriter sw = new StreamWriter(inboxPath, false);
            string json = JsonConvert.SerializeObject(playerInbox, Formatting.Indented);
            sw.Write(json);
            sw.Close();
        }

        private void Load()
        {
            if (File.Exists(inboxPath))
            {
                playerInbox = JsonConvert.DeserializeObject<List<Email>>(File.ReadAllText(inboxPath));
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnGameSave -= Save;
            GameEvent.OnGameLoad -= Load;
        }
    }
}