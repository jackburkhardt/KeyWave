// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// An item asset. In Chat Mapper, items are usually used to track the status of items in the 
    /// simulation. You can still do this in the Dialogue System; however the QuestLog class gives 
    /// you the option of using the item table to track quest log information instead. (See @ref 
    /// questLogSystem)
    /// </summary>
    [System.Serializable]
    public class Item : Asset
    {

        /// <summary>
        /// Gets or sets the field 'Is Item' which indicates whether this asset is an item
        /// or a quest.
        /// </summary>
        /// <value>
        /// <c>true</c> if asset is actually an item; <c>false</c> if the asset is actually
        /// a quest.
        /// </value>
        public bool IsItem
        {
            get { return LookupValue(DialogueSystemFields.ItemType) == "Item"; }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Item");
            }
        }
        
        public bool IsQuest
        {
            get { return LookupValue(DialogueSystemFields.ItemType) == "Quest"; }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Quest");
            }
        }
        
        public bool IsAction
        {
            get { return LookupValue(DialogueSystemFields.ItemType) == "Action"; }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Action");
            }
        }
        
        public bool IsStatic
        {
            get { return LookupBool(DialogueSystemFields.IsStatic); }
            set
            {
                Field.SetValue(fields, DialogueSystemFields.IsStatic, value);
            }
        }
        
        public bool IsEmail
        {
            get { return LookupValue(DialogueSystemFields.ItemType) == "Email"; }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Email");
            }
        }
        
        public bool IsTutorial
        {
            get { return LookupValue(DialogueSystemFields.ItemType) == "Tutorial"; }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Tutorial");
            }
        }
        
        public bool IsContact
        {
            get {  return LookupValue(DialogueSystemFields.ItemType) == "Contact";  }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Contact");
            }
        }

        public bool IsPointCategory
        {
            get {  return LookupValue(DialogueSystemFields.ItemType) == "Points Category";  }
            set
            {
                if (value) Field.SetValue(fields, DialogueSystemFields.ItemType, "Points Category");
            }
        }
        
        public int RepeatCount
        {
            get { return LookupInt(DialogueSystemFields.RepeatCount); }
            set { Field.SetValue(fields, DialogueSystemFields.RepeatCount, value); }
        }
        
        public Texture2D icon = null;
        
      

        /// <summary>
        /// Gets or sets the field 'Group' which is an optional group for quest categorization.
        /// </summary>
        /// <value>The group, or empty string if none.</value>
        public string Group
        {
            get { return LookupValue(DialogueSystemFields.Group); }
            set { Field.SetValue(fields, DialogueSystemFields.Group, value); }
        }

        /// <summary>
        /// Initializes a new Item.
        /// </summary>
        public Item() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceItem">Source item.</param>
        public Item(Item sourceItem) : base(sourceItem as Asset) { }

        /// <summary>
        /// Initializes a new Item copied from a Chat Mapper item.
        /// </summary>
        /// <param name='chatMapperItem'>
        /// The Chat Mapper item.
        /// </param>
        public Item(ChatMapper.Item chatMapperItem)
        {
            Assign(chatMapperItem);
        }

        /// <summary>
        /// Copies a Chat Mapper item.
        /// </summary>
        /// <param name='chatMapperItem'>
        /// The Chat Mapper item.
        /// </param>
        public void Assign(ChatMapper.Item chatMapperItem)
        {
            if (chatMapperItem != null) Assign(chatMapperItem.ID, chatMapperItem.Fields);
        }

    }

}
