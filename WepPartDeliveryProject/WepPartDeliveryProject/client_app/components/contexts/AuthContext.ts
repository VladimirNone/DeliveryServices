import React from "react"

export const AuthContext = React.createContext<authContextProps>({
    isAdmin: false,
    isClient: false,
    isDeliveryMan: false,
    isKitchenWorker: false,
    isAuth: false,
    toggleIsAuthed: ()=>{},
  });