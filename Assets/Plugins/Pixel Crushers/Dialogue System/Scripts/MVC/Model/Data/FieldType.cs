// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Data types used by the Field class.
    /// </summary>
    public enum FieldType
    {

        /// <summary>
        /// Text
        /// </summary>
        Text,

        /// <summary>
        /// Number (integer or float)
        /// </summary>
        Number,

        /// <summary>
        /// Boolean (true or false)
        /// </summary>
        Boolean,

        /// <summary>
        /// A text string in the format [file1, file2, ...]
        /// </summary>
        Files,

        /// <summary>
        /// Localization asset
        /// </summary>
        Localization,

        /// <summary>
        /// Actor ID
        /// </summary>
        Actor,

        /// <summary>
        /// Item ID
        /// </summary>
        Item,

        /// <summary>
        /// Location ID
        /// </summary>
        Location,
        
        //custom
        
        /// <summary>
        /// Formatted as "ConversationID,SubtitleID"
        /// </summary>
        Node,
        
        /// <summary>
        /// Formatted as "hours:seconds"
        /// </summary>
        Time,
        
        Points,
        /// <summary>
        /// Formatted as "pointsType:pointsValue"
        /// </summary>
        
        Timespan,
        /// <summary>
        /// Formatted as "value:unit"
        /// </summary>
        ///
        QuestState,
        
        Color,
        
        Relationship,
        
        ActorState,
        
        Vector2
    }

}
