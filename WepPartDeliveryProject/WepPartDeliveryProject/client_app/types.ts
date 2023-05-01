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

interface jsonTokenInfo {
    jwtToken: string,
    validTo: Date,
    roleName: string,
}

interface authContextProps {
    JwtTokenIsValid: () => boolean,
    UpdateJwtToken: () => Promise<void>,
}

interface profileInfo {
    login:string,
    phoneNumber:string | null,
    name:string,
    born:Date | null,

    // Client props
    bonuses:number | null,

    //KitchenWorker props
    jobTitle:string | null,

    //Admin props

}