// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A location asset. In Chat Mapper, locations are usually used to track the information about 
    /// locations within the simulation. The dialogue system doesn't do anything with locations, 
    /// but you're free to use them in your Lua code.
    /// </summary>
    [System.Serializable]
    public class Location : Asset
    {

        /// <summary>
        /// Initializes a new Location.
        /// </summary>
        public Location() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceLocation">Source location.</param>
        public Location(Location sourceLocation) : base(sourceLocation as Asset) { }

        /// <summary>
        /// Initializes a new Location copied from a Chat Mapper location asset.
        /// </summary>
        /// <param name='chatMapperLocation'>
        /// The Chat Mapper location.
        /// </param>
        public Location(ChatMapper.Location chatMapperLocation)
        {
            Assign(chatMapperLocation);
        }
        /// <summary>
        /// Copies a Chat Mapper location asset.
        /// </summary>
        /// <param name='chatMapperLocation'>
        /// The Chat Mapper location.
        /// </param>
        public void Assign(ChatMapper.Location chatMapperLocation)
        {
            if (chatMapperLocation != null) Assign(chatMapperLocation.ID, chatMapperLocation.Fields);
        }
        
        public bool IsSublocation
        {
            get { return LookupBool(DialogueSystemFields.IsSublocation); }
            set { Field.SetValue(fields, DialogueSystemFields.IsSublocation, value); }
        }

        public int RootID => IsSublocation ? LookupInt("Parent Location") : id;
        
        public Location GetRootLocation( DialogueDatabase database = null)
        {
            database ??= DialogueManager.masterDatabase;
            return database.GetLocation(RootID);
        }

    }

}
