type footerPanelInfo = {
    panelName: string,
    panelItems: Array<linkPanelItem>,
}

type linkPanelItem = {
    itemName: string,
    itemHref: string,
}

type dishClientInfo = {
    images: Array<string>,
    name: string,
    description: string,
    price : number,
    id: string,
    DeleteCartFromList: (dishId:string)=>void,
}

type dishListProps = {
    dishes: Array<dishClientInfo>,
}

type categoryItem = {
    id: string,
    name: string,
    linkName: string,
    categoryNumber: number,
}