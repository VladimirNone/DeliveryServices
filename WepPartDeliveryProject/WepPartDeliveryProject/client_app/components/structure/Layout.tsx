import {FC, ReactNode, useEffect, useState} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";
import React from "react";
import { RoleContext } from "../contexts/RoleContext";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  {
    const [isAuthed, setIsAuthed] = useState(false);
    const [role, setRole] = useState("User");

    const DropJwtToken = () => {
        setRole("User");
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
        //Если с jwt токеном все ок, то нет смысла его обновлять
        //Если все плохо, то удаляем его
        if(JwtTokenIsValid() && isAuthed != false)
            return;

        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/auth/refreshAccessToken`, {
            method: "POST",
            credentials: "include"
        });
    
        if(resp.ok){
            const token = await resp.json() as jsonTokenInfo;
//ВОТ ЭТО ПОД БОЛЬШИМ ВОПРОСОМ!!!!!!!!!!!!!
//роль стоит получать также тут и к тому же, хранить ее в куки. куки защищены
            setRole(token.roleName);
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
    });

    return (
        <>
            <Header isAuthed={isAuthed} dropJwtToken={DropJwtToken}/>
            <MainNavbar isAdmin={role == "Admin"} />
            <RoleContext.Provider value = {{ isAdmin: role == "Admin"}}>
                {children}
            </RoleContext.Provider>
            <Footer />
        </>
    );
};

export default Layout;