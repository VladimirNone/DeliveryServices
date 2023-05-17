import { GetStaticProps } from "next";
import ClientLayout from "@/components/structure/ClientLayout";
import { FC, useState } from "react";
import CustomMap from "@/components/mapComponents/customMap";
import { fromLonLat } from "ol/proj";
import Layers from "@/components/mapComponents/mapLayers/Layers";
import TileLayer from "@/components/mapComponents/mapLayers/TitleLayer";
import * as olSource from "ol/source";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories,
        }
    }
}


const MapPage: FC<{ categories: categoryItem[]}> = ({ categories}) => {
    const [center, setCenter] = useState([48.003456, 37.802683]);
    const [zoom, setZoom] = useState(9);

    return (
        <ClientLayout categories={categories}>
            <CustomMap zoom={zoom} center={fromLonLat(center)}>
            <Layers>
                <TileLayer
                    source={new olSource.OSM()}
                    zIndex={0}
                />
            </Layers>
            </CustomMap>
        </ClientLayout>
    )
}



export default MapPage;