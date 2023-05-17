import { FC, useContext, useEffect } from "react";
import OLTileLayer from "ol/layer/Tile";
import { MapContext } from "@/components/contexts/MapContext";
import * as ol from "ol";
import { Options } from "ol/layer/Layer";

interface mapContextProps {
    map: ol.Map | null,
}

const TileLayer:FC<Options<any>> = ({ source, zIndex = 0 }) => {
  const { map } = useContext<mapContextProps>(MapContext); 
  useEffect(() => {
    if (!map) return;
    
    let tileLayer = new OLTileLayer({
      source,
      zIndex,
    });
    map.addLayer(tileLayer);
    tileLayer.setZIndex(zIndex);
    return () => {
      if (map) {
        map.removeLayer(tileLayer);
      }
    };
  }, [map]);
  return null;
};
export default TileLayer;