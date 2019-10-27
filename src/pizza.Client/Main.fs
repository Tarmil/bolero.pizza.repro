module pizza.Client.Main

open Microsoft.AspNetCore.Components.Routing
open Microsoft.JSInterop
open Bolero.Remoting
open Elmish

type PizzaSpecial =
    { Description: string
      ImageUrl: string
      Name: string
      FormattedBasePrice: string }
type PizzaService =
    { getSpecials : unit -> Async<PizzaSpecial list> }
    interface IRemoteService with
        member this.BasePath = "/pizza"

type Model = { specials: PizzaSpecial list }
type Message = DataReceived of PizzaSpecial list

let initModel (remote : PizzaService) =
    { specials = ( [  ] : PizzaSpecial list) }  ,
    Cmd.ofAsync  remote.getSpecials ()  (fun  e -> DataReceived e) ( fun e -> failwith "" )

let update remote message model =
    match message with
    | DataReceived d -> { model with specials = d}, Cmd.none

open Bolero.Html
open Bolero
type MainLayout = Template<"wwwroot\MainLayout.html">
type PizzaCards = Template<"wwwroot\PizzaCards.html">

type ViewItem() =
    inherit ElmishComponent<PizzaSpecial, Message>()

    override __.View special dispatch =
        if  (special |> box |> isNull)then empty
        else
        PizzaCards.Item()
            .description(special.Description)
            .imageurl(special.ImageUrl)
            .name(special.Name)
            .price(special.FormattedBasePrice)
            .Elt()

let view ( model : Model) dispatch =
    let content =
        PizzaCards()
            .Items(forEach model.specials <| fun i ->
                //text (i.ToString()))
                ecomp<ViewItem,_,_> i dispatch)
           .Elt()
    MainLayout()
        .Body(content)
        .Elt()


type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let remote = this.Remote<PizzaService>()
        let update = update remote
        let init = initModel remote
        Program.mkProgram (fun _ -> init) update view