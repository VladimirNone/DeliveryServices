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
    DeleteCardFromList: (dishId:string)=>void,
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
    roleNames: string[],
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

interface orderCardInfo {
    id: string,
    price: number,
    sumWeight: number,
    deliveryAddress: string,
    DeleteOrder: (orderId:string)=>void,
    MoveOrderToNextStage: (orderId:string, orderStateId:string)=>void,
    MoveOrderToPreviousStage: (orderId:string, orderStateId:string)=>void,
    story?: orderState[],
}

interface orderedDishClientInfo {
    count:number,
    orderId:string,
    dishInfo:dishCartInfo,
}

interface orderInfo {
    order: orderCardInfo,
    orderedDishes: orderedDishClientInfo[],
}

interface authContextProps {
    isAdmin: boolean,
    isAuth: boolean,
    toggleIsAuthed: ()=>void,
}

interface orderState {
    timeStartState: Date,
    comment: string,
    orderStateId: string,
    numberOfStage: number,
    nameOfState: string,
    descriptionForClient: string,
}