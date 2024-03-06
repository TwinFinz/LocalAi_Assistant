namespace LocalAiAssistant
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
#if DEBUG
            IntroLabel.FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span
                    {
                        Text = "##### DEBUG MODE #####\n\n",
                        TextColor = Colors.Red
                    },
                    new Span
                    {
                        Text = $"{IntroLabel.Text}"
                    }
                }
            };
#endif
        }
    }

}
