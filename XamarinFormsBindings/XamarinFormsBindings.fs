namespace XamarinFormsBindings

open Xamarin.Forms
open System
open System.Diagnostics
open System.Linq
open System.Collections.ObjectModel
open System.Windows.Input
open System.ComponentModel


module LabelSample =
    
    type MyPage(viewmodel: obj) as self =
        inherit ContentPage(Title = "First sample")
    
        let label =
            new Label()
    
        do
            label.SetBinding(Label.TextProperty, "Text")
            self.BindingContext <- viewmodel
            self.Content <- label
    
    type MyPageViewModel() =
        member self.Text with get() = "Hey"

module LabelEntrySample =

    type MyPage(viewmodel: obj) as self =
        inherit ContentPage(Title = "First sample")

        let label = new Label()
        let entry = new Entry()
        let layout =
            let layout = new StackLayout()
            layout.Children.Add(label)
            layout.Children.Add(entry)
            layout

        do
            label.SetBinding(Label.TextProperty, "Text")
            entry.SetBinding(Entry.TextProperty, "Text")
            self.BindingContext <- viewmodel
            self.Content <- layout
    
    type MyPageViewModel() =

        let mutable text = "Default text"

        member self.Text 
            with get() = text
            and set value = 
                text <- value
                Debug.WriteLine("Value set: {0}", value)
          
module BiggerSample =

    type Person = {
        Name: string
        Age: int
    }

    type ViewModelBase() =
        let propertyChanging = new Event<PropertyChangingEventHandler, PropertyChangingEventArgs>()
        let propertyChanged  = new Event<PropertyChangedEventHandler,  PropertyChangedEventArgs>()

        interface INotifyPropertyChanged with
            [<CLIEvent>]
            member self.PropertyChanged = propertyChanged.Publish
        
        member self.PropertyChanging = propertyChanging.Publish

        member self.OnPropertyChanging name =
            propertyChanging.Trigger(self, new PropertyChangingEventArgs(name))

        member self.OnPropertyChanged name =
            propertyChanged.Trigger(self, new PropertyChangedEventArgs(name))

    type ViewModel() =
        inherit ViewModelBase()

        let mutable name = ""

        let persons =
            new ObservableCollection<Person>(
                [ { Name = "Kim"; Age = 29 }
                  { Name = "Tom"; Age = 29 }
                  { Name = "Sam"; Age = 29 } ]
            )

        let add name =
            persons.Add({ Name = name; Age = 10 })

        member self.List 
            with get() =
                persons

        member self.Name
            with get() = 
                name
            and set value = 
                base.OnPropertyChanging "Name"
                name <- value
                base.OnPropertyChanged "Name"

        member self.AddCommand
            with get() =
                new Command<string>(fun name -> add name)

        member self.SetEntryCommand
            with get() =
                new Command<unit>(fun () -> self.Name <- "Hello word")

    type Cell() as self =
        inherit ViewCell()

        let name = new Label()
        let age  = new Label()

        let layout =
            let layout = new StackLayout(Orientation = StackOrientation.Horizontal)
            layout.Children.Add(name)
            layout.Children.Add(age)
            layout
        
        do
            name.SetBinding(Label.TextProperty, "Name")
            age.SetBinding(Label.TextProperty, "Age", stringFormat = "{0} years old")
            self.View <- layout

    type Root(viewModel: obj) as self =
        inherit ContentPage(Title = "Databindings")

        let list  = new ListView(ItemTemplate = new DataTemplate(typeof<Cell>))
        let btn   = new Button(Text = "Add")
        let test  = new Button(Text = "Set entry to Hello world")
        let entry = new Entry()

        let layout = 
            let layout = new StackLayout()
            layout.Children.Add(list)
            layout.Children.Add(entry)
            layout.Children.Add(btn)
            layout.Children.Add(test)
            layout

        do
            list.SetBinding(ListView.ItemsSourceProperty, "List")
            list.ItemTemplate.SetBinding(TextCell.TextProperty, "Name")
            entry.SetBinding(Entry.TextProperty, "Name")
            btn.SetBinding(Button.CommandProperty, "AddCommand")
            btn.SetBinding(Button.CommandParameterProperty, "Name")
            test.SetBinding(Button.CommandProperty, "SetEntryCommand")

            self.SetBinding(ContentPage.TitleProperty, "Name")
            base.Content <- layout
            base.BindingContext <- viewModel

type App() = 
    inherit Application()

    do 
        //base.MainPage <- new MyPage(new MyPageViewModel())
        //base.MainPage <- Root(new ViewModel())
        base.MainPage <- LabelEntrySample.MyPage(new LabelEntrySample.MyPageViewModel())

