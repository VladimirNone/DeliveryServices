import {FC, ReactNode, useEffect, useState} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";
import React from "react";
import { AuthContext as AuthContext } from "../contexts/AuthContext";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  {
    const [isAuthed, setIsAuthed] = useState(false);
    const [roles, setRoles] = useState(["User"]);

    const DropJwtToken = () => {
        setRoles(["User"]);
        setIsAuthed(false);

        localStorage.removeItem("jwtToken");
        localStorage.removeItem("jwtTokenValidTo");
    };

    const JwtTokenIsValid = ():boolean => {
        const jwtTokenValidTo: string | null = localStorage.getItem("jwtTokenValidTo");

        if(jwtTokenValidTo == null || new Date(jwtTokenValidTo) < new Date()){
            return false;
        }

        return true;
    };

    const UpdateJwtToken = async () => {
        //Если пользователь не авторизован и при этом
        //Если с jwt токеном все ок, то нет смысла его обновлять
        if(JwtTokenIsValid() && isAuthed == true)
            return;

        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/auth/refreshAccessToken`, {
            method: "POST",
            credentials: "include"
        });
    
        if(resp.ok){
            const token = await resp.json() as jsonTokenInfo;

            setRoles(token.roleNames);
            setIsAuthed(true);

            localStorage.setItem("jwtToken", token.jwtToken);
            localStorage.setItem("jwtTokenValidTo", token.validTo.toString());
        }
        else{
            DropJwtToken();
        }
    }

    useEffect(() => {
        UpdateJwtToken();
    },[isAuthed]);

    return (
        <>
            <Header isAuthed={isAuthed} dropJwtToken={DropJwtToken}/>
            <MainNavbar isAdmin={roles.includes("Admin")} />
            <AuthContext.Provider value = {{ isAdmin: roles.includes("Admin"), isAuth: isAuthed, toggleIsAuthed: () => setIsAuthed(true)}}>
                {children}
            </AuthContext.Provider>
            <Footer />
        </>
    );
};

export default Layout;