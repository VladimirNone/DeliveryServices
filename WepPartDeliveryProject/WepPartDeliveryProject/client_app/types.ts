type footerPanelInfo = {
    panelName: string,
    panelItems: Array<linkPanelItem>,
}

type linkPanelItem = {
    itemName: string,
    itemHref: string,
}

type dishClientCardProps = {
    images: Array<string>,
    name: string,
    description: string,
    price : number,
    id: number,
}