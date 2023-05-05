import React from "react"

export const AuthContext = React.createContext<authContextProps>({
    isAdmin: false,
    isAuth: false,
    toggleIsAuthed: ()=>{},
  });