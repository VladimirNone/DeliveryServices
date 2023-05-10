import React from "react"

export const AuthContext = React.createContext<authContextProps>({
    isAdmin: false,
    isKitchenWorker: false,
    isAuth: false,
    toggleIsAuthed: ()=>{},
  });