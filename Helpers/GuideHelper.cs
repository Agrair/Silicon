namespace Silicon.Helpers
{
    static class GuideHelper
    {
        public static readonly string[] Tips = new[]
        {
            "You can set custom embed colors for tags. Use `|help` to see how.",
            "Messaging `|help` to the bot privately will allow you to see admin commands.",
            "Trying to input a user without pinging them, but their username contains spaces? " +
                "Try enclosing their name with quotes: |userinfo \"Username with spaces\"",
            "Trying to input a user without pinging them, but their username contains spaces? " +
                "You can see Discord IDs by first click `User Settings` > `Appearance` > `Developer Mode`. " +
                "You can then right click users and find their ID: |userinfo \"12345678910272\""
        };

        public static readonly string[] GuidePages = new[]
        {
            $"<@{Program.userID}> has a unique currency system. This guide aims to introduce the basics.",
            "First, you need to understand Timeslots. Most commands just have a ratelimit, to prevent spam, but " +
                "some commands that increase your stats have cooldowns. Similar commands share cooldowns, but have " +
                "individual values. For example, `|panforgold` might lock your \"Work\" Timeslot for 30 minutes, but " +
                "`|busker` might lock your \"Work\" Timeslot for 20 minutes.",
            "In order to make money, you can apply for a job using `|profession`. Make sure to pick one you know you can maintain. " +
                "You can get better jobs by increasing your Intelligence, but some jobs just require you to have " +
                "some experience. You can use `|study` to improve your Intelligence. It will ask you to answer trivia, " +
                "use words in sentences or link definitions, and sometimes the occasional hangman.",
            "As you work, you can use your coins to buy various items from the `|shop`. Some help you beneficially to make more " +
                "coins, other increase your Street Cred. Committing larceny and other crimes decreases your Street Cred, " +
                "things like community service increases it, and buying things like a Hoverboard or a Gaming PC can have random " +
                "effects on it."
        };
    };
}
