using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace UserControls.General
{
    /// <summary>
    /// Interaction logic for CustomButton.xaml
    /// </summary>
    public partial class CustomButton : UserControl
    {
        #region Constructor

        public CustomButton()
        {
            InitializeComponent();

			//ButtonText.SizeChanged += SendInitializationEndedEvent;
        }

		#endregion Constructor

        #region Dependency Properties

        [Category("UserControl")]
        public ImageSource ButtonImage
        {
            get { return (ImageSource)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }

        public static readonly DependencyProperty ButtonImageProperty =
            DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(CustomButton), new UIPropertyMetadata(null));

        [Category("UserControl")]
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(double), typeof(CustomButton), new UIPropertyMetadata(23d));

        [Category("UserControl")]
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(CustomButton), new UIPropertyMetadata(23d));

        [Category("UserControl")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CustomButton));

        /// <summary>
        /// Margin of the Image
        /// </summary>
        public static readonly DependencyProperty ImageMarginProperty
            = DependencyProperty.Register("ImageMargin", typeof(Thickness), typeof(CustomButton), 
                new UIPropertyMetadata(new Thickness(-2,-3,0,0)));

        [Category("User Control")]
        [Description("Margin of the Image")]
        /// <summary>
        /// Margin of the Image
        /// </summary>
        public Thickness ImageMargin
        {
            get { return (Thickness)GetValue(ImageMarginProperty); }
            set { SetValue(ImageMarginProperty, value); }
        }

        /// <summary>
        /// Visibility of the Button's Text.
        /// </summary>
        public static readonly DependencyProperty TextVisibilityProperty
            = DependencyProperty.Register("TextVisibility", typeof(Visibility), typeof(CustomButton),
                new UIPropertyMetadata(Visibility.Visible, TextVisibilityChangedCallback));

        [Category("User Control")]
        [Description("Visibility of the Button's Text.")]
        /// <summary>
        /// Visibility of the Button's Text.
        /// </summary>
        public Visibility TextVisibility
        {
            get { return (Visibility)GetValue(TextVisibilityProperty); }
            set { SetValue(TextVisibilityProperty, value); }
        }

        /// <summary>
        /// Command of the Button.
        /// </summary>
        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        [Category("User Control")]
        [Description("Command or Action to be Executed when the Button is clicked.")]
        /// <summary>
        /// Command of the Button.
        /// </summary>
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(CustomButton));

        /// <summary>
        /// Button's Command Parameter.
        /// </summary>
        public object ButtonCommandParameter
        {
            get { return GetValue(ButtonCommandParameterProperty); }
            set { SetValue(ButtonCommandParameterProperty, value); }
        }

        [Category("User Control")]
        [Description("Button's Command Parameter.")]
        /// <summary>
        /// Button's Command Parameter.
        /// </summary>
        public static readonly DependencyProperty ButtonCommandParameterProperty =
            DependencyProperty.Register("ButtonCommandParameter", typeof(object), typeof(CustomButton));

        /// <summary>
        /// Visibility of the Image.
        /// </summary>
        public static readonly DependencyProperty ImageVisibilityProperty
            = DependencyProperty.Register("ImageVisibility", typeof(Visibility), typeof(CustomButton), 
				new UIPropertyMetadata(Visibility.Visible));

        [Category("User Control")]
        [Description("Visibility of the Image..")]
        /// <summary>
        /// Visibility of the Image.
        /// </summary>
        public Visibility ImageVisibility
        {
            get { return (Visibility)GetValue(ImageVisibilityProperty); }
            set { SetValue(ImageVisibilityProperty, value); }
        }

        /// <summary>
        /// Margin of the Text
        /// </summary>
        public static readonly DependencyProperty TextMarginProperty
            = DependencyProperty.Register("TextMargin", typeof(Thickness), typeof(CustomButton), 
                new UIPropertyMetadata(new Thickness(5,0,0,0)));

        [Category("User Control")]
        [Description("Margin of the Text")]
        /// <summary>
        /// Margin of the Text
        /// </summary>
        public Thickness TextMargin
        {
            get { return (Thickness)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }

        /// <summary>
        /// Property used for Setting the Imagen Left or Right to the Text.
        /// TRUE: Image is Right to the Text.
        /// FALSE: Image is Left to the Text.
        /// </summary>
        public static readonly DependencyProperty ImageAtRightProperty
            = DependencyProperty.Register("ImageAtRight", typeof(bool), typeof(CustomButton)
            , new PropertyMetadata(ImageAtRightChangedCallback));

        [Category("User Control")]
        [Description("Indicates if the Image is at Text's Right.")]
        /// <summary>
        /// Property used for Setting the Imagen Left or Right to the Text.
        /// TRUE: Image is Right to the Text.
        /// FALSE: Image is Left to the Text.
        /// </summary>
        public bool ImageAtRight
        {
            get { return (bool)GetValue(ImageAtRightProperty); }
            set { SetValue(ImageAtRightProperty, value); }
        }

        [Category("UserControl")]
        public string ButtonTooltip
        {
            get { return (string)GetValue(ButtonTooltipProperty); }
            set { SetValue(ButtonTooltipProperty, value); }
        }

        public static readonly DependencyProperty ButtonTooltipProperty =
            DependencyProperty.Register("ButtonTooltip", typeof(string), typeof(CustomButton));

        /// <summary>
        /// Padding of the Button
        /// </summary>
        public static readonly DependencyProperty ButtonPaddingProperty
            = DependencyProperty.Register("ButtonPadding", typeof(Thickness), typeof(CustomButton),
                new UIPropertyMetadata(new Thickness(8,5,8,5)));

        [Category("User Control")]
        [Description("Padding of the Button")]
        /// <summary>
        /// Padding of the Button
        /// </summary>
        public Thickness ButtonPadding
        {
            get { return (Thickness)GetValue(ButtonPaddingProperty); }
            set { SetValue(ButtonPaddingProperty, value); }
        }

        /// <summary>
        /// ObjectName to find the Texts of the ToolTip of the Button
        /// </summary>
        public static readonly DependencyProperty ObjectNameProperty
            = DependencyProperty.Register("ObjectName", typeof(string), typeof(CustomButton));

        /// <summary>
        /// ObjectName to find the Texts of the ToolTip of the Button
        /// </summary>
        public string ObjectName
        {
            get { return (string)GetValue(ObjectNameProperty); }
            set { SetValue(ObjectNameProperty, value); }
        }

        /// <summary>
        /// Label to find the Texts of the ToolTip of the Button
        /// </summary>
        public static readonly DependencyProperty LabelToolTipProperty
            = DependencyProperty.Register("LabelToolTip", typeof(string), typeof(CustomButton));

        /// <summary>
        /// Label to find the Texts of the ToolTip of the Button
        /// </summary>
        public string LabelToolTip
        {
            get { return (string)GetValue(LabelToolTipProperty); }
            set { SetValue(LabelToolTipProperty, value); }
        }

        /// <summary>
        /// HorizontalContentAlignment of the Button
        /// </summary>
        public static readonly DependencyProperty ButtonHorizontalContentAlignmentProperty
            = DependencyProperty.Register("ButtonHorizontalContentAlignment", typeof(HorizontalAlignment), typeof(CustomButton),
                new UIPropertyMetadata(HorizontalAlignment.Left));

        /// <summary>
        /// Label to find the Texts of the ToolTip of the Button
        /// </summary>
        public string ButtonHorizontalContentAlignment
        {
            get { return (string)GetValue(ButtonHorizontalContentAlignmentProperty); }
            set { SetValue(ButtonHorizontalContentAlignmentProperty, value); }
        }

        /// <summary>
        /// Background of the Button
        /// </summary>
        public static readonly DependencyProperty ButtonBackgroundProperty
            = DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(CustomButton),
                new UIPropertyMetadata(Brushes.CornflowerBlue));

        /// <summary>
        /// Background of the Button
        /// </summary>
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }

        /// <summary>
        /// Font Size of the Button
        /// </summary>
        public static readonly DependencyProperty ButtonFontSizeProperty
            = DependencyProperty.Register("ButtonFontSize", typeof(double), typeof(CustomButton),
                new UIPropertyMetadata(15d));

        /// <summary>
        /// Font Size of the Button
        /// </summary>
        public double ButtonFontSize
        {
            get { return (double)GetValue(ButtonFontSizeProperty); }
            set { SetValue(ButtonFontSizeProperty, value); }
        }

        #endregion Dependency Properties

        #region Methods

        private void SetColumnControls()
        {
            if (ImageAtRight)
            {
                ButtonText.SetValue(Grid.ColumnProperty, 0);
                ImageInButton.SetValue(Grid.ColumnProperty, 1);
            }
            else if (!ImageAtRight)
            {
                ButtonText.SetValue(Grid.ColumnProperty, 1);
                ImageInButton.SetValue(Grid.ColumnProperty, 0);
            }
        }

        /// <summary>
        /// Event executed when the ImageAtRight Property's Value is Changed.
        /// </summary>
        /// <param name="d">(this).</param>
        /// <param name="e">Event Parameters.</param>
        private static void ImageAtRightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CustomButton;

            if (control == null)
            {
                return;
            }

            control.SetColumnControls();
        }

		/// <summary>
		/// Event executed when the TextVisibility Property's Value is Changed.
		/// </summary>
		/// <param name="d">(this).</param>
		/// <param name="e">Event Parameters.</param>
		private static void TextVisibilityChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as CustomButton;

			if (control == null)
			{
				return;
			}

			if((Visibility)e.NewValue == Visibility.Hidden || (Visibility)e.NewValue == Visibility.Collapsed)
			{
				control.ImageMargin = new Thickness(0);
			}
			else
			{
				control.ImageMargin = new Thickness(-2, -3, 0, 0);
			}
		}

        public override void LoadedEventMethod()
        {
            base.LoadedEventMethod();

            if(ButtonBackground == Brushes.Orange)
            {
                ButtonC.Background = ButtonBackground;
                ButtonC.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F28500");
                ButtonC.Style = (Style)Resources["OrangeButtonRoundCorner"];
            }
            else if(ButtonBackground == Brushes.Transparent)
            {
                ButtonC.Style = null;
                ButtonC.Background = Brushes.Transparent;
                ButtonC.BorderThickness = new Thickness(0);
            }
        }

        #endregion Methods

        private async void ButtonC_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
			await ButtonC_ToolTipOpeningAsync(sender, e);
        }

		private async Task ButtonC_ToolTipOpeningAsync(object sender, ToolTipEventArgs e)
		{
			if (!string.IsNullOrEmpty(ObjectName) && !string.IsNullOrEmpty(LabelToolTip))
			{
				var systemTexts = await SystemTextManager.SearchByLanguage(ObjectName);
				var toolTip = systemTexts.FirstOrDefault(x => x.Label == LabelToolTip);

				if (toolTip != null && !string.IsNullOrEmpty(toolTip.TextValue))
				{
					ButtonTooltip = toolTip.TextValue;
				}
			}
		}

	}
}
