import {FC, ReactNode, useEffect, useState} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";
import React from "react";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  {
    const [isAuthed, setIsAuthed] = useState(false);
    const [role, setRole] = useState("none");

    const JwtTokenIsValid = ():boolean => {
        const jwtTokenValidToNow: string | null = localStorage.getItem("jwtTokenValidTo");

        if(jwtTokenValidToNow == null || new Date(jwtTokenValidToNow) < new Date()){
            setIsAuthed(false);
            return false;
        }

        setIsAuthed(true);
        return true;
    };

    const UpdateJwtToken = async () => {

        if(JwtTokenIsValid())
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
            setIsAuthed(false);
        }
    }

    useEffect(() => {
        UpdateJwtToken();
    });

    return (
        <>
            <Header isAuthed={isAuthed}/>
            <MainNavbar isAdmin={role == "Admin"} />
            {children}
            <Footer />
        </>
    );
};

export default Layout;