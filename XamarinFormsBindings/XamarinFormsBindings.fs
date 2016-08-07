namespace XamarinFormsBindings

open Xamarin.Forms
open System
open System.Diagnostics
open System.Linq
open System.Collections
open System.Collections.ObjectModel
open System.Windows.Input
open System.ComponentModel

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


module LabelSample =
    
    type MyPage(viewmodel: obj) as self =
        inherit ContentPage(Title = "Label sample")
    
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
        inherit ContentPage(Title = "Label and entry sample")

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
        inherit ViewModelBase()

        let mutable text = "Default text"

        member self.Text 
            with get() = text
            and set value = 
                base.OnPropertyChanging "Text"
                text <- value
                Debug.WriteLine("Value set: {0}", value)
                base.OnPropertyChanged "Text"
          
module ListViewSimpleSample =

    type MyTextCell() as self =
        inherit ViewCell()

        let name = new Label()
        let age = new Label()
        let layout =
            let layout = new StackLayout(Orientation = StackOrientation.Horizontal)
            layout.Children.Add(name)
            layout.Children.Add(age)
            layout

        do
            name.SetBinding(Label.TextProperty, "Name", stringFormat = "{0},")
            age.SetBinding(Label.TextProperty, "Age", stringFormat = "{0} years old")
            self.View <- layout

    type MyPage(viewmodel: obj) as self =
        inherit ContentPage(Title = "ListView sample")

        //let listView = 
        //    new ListView(ItemTemplate = new DataTemplate(typeof<TextCell>))

        let listView = 
            new ListView(ItemTemplate = new DataTemplate(fun () -> box <| new MyTextCell()))

        do
            self.BindingContext <- viewmodel
            listView.SetBinding(ListView.ItemsSourceProperty, "List")
            listView.ItemTemplate.SetBinding(TextCell.TextProperty, "Name")
            self.Content <- listView
    
    type MyPageViewModel() =

        member self.List 
            with get() = 
                [ { Name = "Greg"; Age = 29 }
                  { Name = "Tom"; Age = 13 }
                  { Name = "Sam"; Age = 5 } ]

module ObservableCollectionSample =
   
   type MyPageViewModel() as self =

       let list =
           new ObservableCollection<PersonViewModel>(
               [ { Name = "Greg"; Age = 29 }
                 { Name = "Tom"; Age = 13 }
                 { Name = "Sam"; Age = 5 } ]
               |> List.map(fun p -> new PersonViewModel(self, p.Name, p.Age)))

       member self.List
           with get() =
               list

       member self.Add
           with get() =
               new Command(fun () -> list.Add(new PersonViewModel(self, "New person", 0)))
       
    and PersonViewModel(parent: MyPageViewModel, name, age) =
        inherit ViewModelBase()

        let mutable name = name
        let mutable age = age

        member self.Name
            with get() = name
            and set value = 
                name <- value
                base.OnPropertyChanged "Name"

        member self.Age
            with get() = age
            and set value = 
                age <- value
                base.OnPropertyChanged "Name"

        member self.Remove
            with get() =
                new Command<PersonViewModel>(fun p -> parent.List.Remove p |> ignore)
        
        new(parent:MyPageViewModel) = 
            new PersonViewModel(parent, "", 0)
                

    type MyTextCell() as self =
        inherit ViewCell()

        let name = new Label()
        let age = new Label()
        let delete = new Button(Text = "Remove")
        let layout =
            let layout = new StackLayout(Orientation = StackOrientation.Horizontal)
            layout.Children.Add(name)
            layout.Children.Add(age)
            layout.Children.Add(delete)
            layout

        do
            name.SetBinding(Label.TextProperty, "Name", stringFormat = "{0},")
            age.SetBinding(Label.TextProperty, "Age", stringFormat = "{0} years old")

            delete.SetBinding(Button.CommandProperty, "Remove")
            delete.SetBinding(Button.CommandParameterProperty, ".")

            self.View <- layout

    type MyPage(viewmodel: obj) as self =
        inherit ContentPage(Title = "Observable collection sample")

        let listView = new ListView(ItemTemplate = new DataTemplate(typeof<MyTextCell>))
        let btn      = new Button(Text = "Add new")

        let layout =
            let layout = new StackLayout()
            layout.Children.Add(listView)
            layout.Children.Add(btn)
            layout

        do
            self.BindingContext <- viewmodel
            listView.SetBinding(ListView.ItemsSourceProperty, "List")
            btn.SetBinding(Button.CommandProperty, "Add")
            self.Content <- layout

type App() = 
    inherit Application()

    do 
        //base.MainPage <- new LabelSample.MyPage(new LabelSample.MyPageViewModel())
        //base.MainPage <- new LabelEntrySample.MyPage(new LabelEntrySample.MyPageViewModel())
        //base.MainPage <- new ListViewSimpleSample.MyPage(new ListViewSimpleSample.MyPageViewModel())
        base.MainPage <- new ObservableCollectionSample.MyPage(new ObservableCollectionSample.MyPageViewModel())

