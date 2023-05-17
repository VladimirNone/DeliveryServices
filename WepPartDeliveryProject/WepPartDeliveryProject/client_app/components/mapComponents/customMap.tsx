import { MapContext } from "../contexts/MapContext";
import { FC, ReactNode, useEffect, useRef, useState } from "react";
import * as ol from "ol";
import { MapOptions } from "ol/Map";

const CustomMap: FC<{ children: ReactNode, zoom: number, center: Array<number> }> = ({ children, zoom, center }) => {
    const mapRef = useRef<HTMLDivElement | null>(null);
    const [map, setMap] = useState<ol.Map | null>(null);
    
    // on component mount
    useEffect(() => {
        const options:MapOptions = {
            view: new ol.View({ zoom, center }),
            layers: [],
            controls: [],
            overlays: []
        };
        const mapObject = new ol.Map(options);
        mapObject.setTarget(mapRef.current as string | HTMLElement | undefined);
        setMap(mapObject);
        return () => mapObject.setTarget(undefined);
    }, []);
    // zoom change handler
    useEffect(() => {
        if (!map) return;
        map.getView().setZoom(zoom);
    }, [zoom]);
    // center change handler
    useEffect(() => {
        if (!map) return;
        map.getView().setCenter(center)
    }, [center])
    return (
        <MapContext.Provider value={{ map }}>
            <div ref={mapRef} style={{height:'500px'}} >
                {children}
            </div>
        </MapContext.Provider>
    )
}
export default CustomMap;