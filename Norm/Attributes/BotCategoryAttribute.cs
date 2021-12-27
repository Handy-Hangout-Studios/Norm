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
        GENERAL,
        EVENTS_AND_ANNOUNCEMENTS,
        CONFIG_AND_INFO,
        MODERATION,
        WEB_NOVEL,
        TIME,
        MISCELLANEOUS,
        SCHEDULING,
        EVALUATION,
    }

    public static class BotCategoryExtensionMethods
    {
        public static string ToCategoryString(this BotCategory category)
        {
            return category switch
            {
                BotCategory.GENERAL => "General",
                BotCategory.EVENTS_AND_ANNOUNCEMENTS => "Events and Announcements",
                BotCategory.CONFIG_AND_INFO => "Configuration and Information",
                BotCategory.MODERATION => "Moderation",
                BotCategory.WEB_NOVEL => "WebNovel",
                BotCategory.TIME => "Time",
                BotCategory.MISCELLANEOUS => "Miscellaneous",
                BotCategory.SCHEDULING => "Scheduling sub-commands",
                BotCategory.EVALUATION => "Evaluation",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
