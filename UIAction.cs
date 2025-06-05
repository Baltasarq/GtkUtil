// Colorado (c) 2015-2025 Baltasar MIT License <baltasarq@gmail.com>


namespace GtkUtil;


public class UIAction {
    public UIAction(string name, string label, string toolTip, Gdk.Pixbuf? icon = null)
    {
        this.Name = name;
        this.Label = label;
        this.ToolTip = toolTip;
        this.icon = icon ?? Gtk.Image.NewFromIconName( "help-faq", Gtk.IconSize.Button ).Pixbuf;

        AccelGroup ??= new Gtk.AccelGroup();

        this.widgets = [];
        this.IsEnabled = true;
        this.Activated += delegate {};
        this.AccelKey = null;
        this.AccelModifier = Gdk.ModifierType.None;
    }

    /// <summary>The name of the action, like "save".</summary>
    public string Name {
        get;
    }

    /// <summary>The label for that action, like "_Save".</summary>
    public string Label {
        get;
    }

    /// <summary>
    /// The tooltip or explanation of the action, as in "Stores your work.".
    /// </summary>
    public string ToolTip {
        get;
    }

    /// <summary>The image for the menu item or tool button.</summary>
    public Gdk.Pixbuf Icon {
        get => this.icon;
        set {
            this.icon = value;
            this.UpdateIcons();
        }
    }

    /// <summary>
    /// Gets the current functionality or sets it in all related widgets.
    /// </summary>
    public bool IsEnabled {
        get {
            return this.enabled;
        }
        set {
            this.enabled = value;
            this.widgets.ForEach( w => w.Sensitive = value );
        }
    }

    /// <summary>The Gtk's synonym of <see cref="IsEnabled"/>.</summary>
    public bool Sensitive {
        get => this.IsEnabled;
        set => this.IsEnabled = value;
    }

    /// <summary>The <see cref="Gdk.Key"/> for this accelerator.</summary>
    /// <value>a Gdk.Key value representing a key; or null if not accelerator is set.</value>
    public Gdk.Key? AccelKey {
        get; private set;
    }

    /// <summary>The <see cref="Gdk.ModifierType" /> for this accelerator.</summary>
    public Gdk.ModifierType AccelModifier {
        get; private set;
    }

    /// <summary>Determines whether this action has an accelerator or not.</summary>
    public bool HasAccelerator {
        get => AccelKey is not null;
    }

    /// <summary>
    /// Change the key accelerator for the widgets of this action.
    /// </summary>
    /// <param name="key">the <see cref="Gdk.Key" />
    ///                        for this accelerator.</param>
    /// <param name="modifier">the <see cref="Gdk.ModifierType" />
    ///                        for this accelerator.</param>
    public void SetAccelerator(Gdk.Key key, Gdk.ModifierType modifier)
    {
        foreach(var w in this.widgets) {
            SetWidgetAccel( w, key, modifier );
        }

        this.AccelKey = key;
        this.AccelModifier = modifier;
    }
    public EventHandler Activated;

    /// <summary>Creates a menuitem related to this action.</summary>
    /// <returns>The <see cref="Gtk.MenuItem"/>.</returns>
    public Gtk.MenuItem CreateMenuItem()
    {
        var toret = new Gtk.MenuItem( this.Label );

        toret.Activated += this.Activated;
        this.NewWidget( toret );
        return toret;
    }

    /// <summary>
    /// Creates a <see cref="Gtk.ToolButton"/>, related to this action.
    /// </summary>
    /// <returns>A <see cref="Gtk.ToolButton"/>.</returns>
    public Gtk.ToolButton CreateToolButton()
    {
        string label = this.Label.Replace( "_", "" );
        var toret = new Gtk.ToolButton( new Gtk.Image( this.Icon ), label );

        toret.Clicked += this.Activated;
        this.NewWidget( toret );
        return toret;
    }

    private void NewWidget(Gtk.Widget w)
    {
        if ( this.AccelKey is not null ) {
            Gdk.Key key = this.AccelKey ?? throw new ArgumentException( "impossible !!??" );
            this.SetWidgetAccel( w, key, this.AccelModifier );
        }

        if ( w is Gtk.ToolButton tb) {
            tb.IconWidget = new Gtk.Image( this.Icon );
        }

        w.Sensitive = this.IsEnabled;
        this.widgets.Add( w );
    }

    private void SetWidgetAccel(Gtk.Widget w, Gdk.Key key, Gdk.ModifierType modifier)
    {
        if ( this.AccelKey is not null ) {
            w.RemoveAccelerator( AccelGroup, (uint) this.AccelKey, this.AccelModifier );
        }

        w.AddAccelerator( "activate", AccelGroup,
                                new Gtk.AccelKey(
                                    key,
                                    modifier,
                                    Gtk.AccelFlags.Visible ));
    }

    private void UpdateIcons()
    {
        foreach(Gtk.Widget w in this.widgets) {
            if ( w is Gtk.ToolButton tb ) {
                tb.IconWidget = new Gtk.Image( this.Icon );
            }
        }
    }

    /// <summary>
    /// The <see cref="AccelGroup"/> for all accelerators of these actions.
    /// </summary>
    static public Gtk.AccelGroup? AccelGroup {
        get; private set;
    }

    private bool enabled;
    private Gdk.Pixbuf icon;
    private readonly List<Gtk.Widget> widgets;
}
