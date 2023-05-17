import React from "react";
import * as ol from "ol";

interface mapContextProps {
    map: ol.Map | null,
}

export const MapContext = React.createContext<mapContextProps>({
    map: null
});