using System;

namespace Norm.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class BotCategoryAttribute : Attribute
    {
        public BotCategory Category { get; set; }

        public BotCategoryAttribute(BotCategory category)
        {
            this.Category = category;
        }
    }

    public enum BotCategory
    {
        General, 
        EventsAndAnnouncements,
        ConfigAndInfo,
        Moderation,
        WebNovel,
        Time,
        Miscellaneous,
        Scheduling,
    }

    public static class BotCategoryExtensionMethods
    {
        public static string ToCategoryString(this BotCategory category) =>
            category switch
            {
                BotCategory.General => "General",
                BotCategory.EventsAndAnnouncements => "Events and Announcements",
                BotCategory.ConfigAndInfo => "Configuration and Information",
                BotCategory.Moderation => "Moderation",
                BotCategory.WebNovel => "WebNovel",
                BotCategory.Time => "Time",
                BotCategory.Miscellaneous => "Miscellaneous",
                BotCategory.Scheduling => "Scheduling sub-commands",
                _ => throw new NotImplementedException(),
            };
    }
}
