interface footerPanelInfo {
    panelName: string,
    panelItems: Array<linkPanelItem>,
}

interface linkPanelItem {
    itemName: string,
    itemHref: string,
}

interface dishClientInfo {
    images: Array<string>,
    name: string,
    description: string,
    price : number,
    id: string,
}

interface dishCartInfo extends dishClientInfo {
    DeleteCartFromList: (dishId:string)=>void,
}

interface dishListProps {
    dishes: Array<dishClientInfo>,
}

interface categoryItem {
    id: string,
    name: string,
    linkName: string,
    categoryNumber: number,
}