// Copyright (c) Pixel Crushers. All rights reserved.

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
            get { return LookupBool(DialogueSystemFields.IsItem); }
            set { Field.SetValue(fields, DialogueSystemFields.IsItem, value); }
        }
        
        public bool IsAction
        {
            get { return LookupBool(DialogueSystemFields.IsAction); }
            set { Field.SetValue(fields, DialogueSystemFields.IsAction, value); }
        }
        
        public bool IsStatic
        {
            get { return LookupBool(DialogueSystemFields.IsStatic) && LookupBool(DialogueSystemFields.IsAction); }
            set { Field.SetValue(fields, DialogueSystemFields.IsStatic, value); }
        }
        
        public int RepeatCount
        {
            get { return LookupInt(DialogueSystemFields.RepeatCount); }
            set { Field.SetValue(fields, DialogueSystemFields.RepeatCount, value); }
        }
        
      

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
