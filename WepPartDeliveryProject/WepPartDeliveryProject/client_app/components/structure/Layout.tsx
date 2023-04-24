import {FC, ReactNode, useState} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";
import { NavbarContext } from "../contexts/Navbar-context";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  {
    const [isAdmin, setIsAdmin] = useState(true);
    const toggleIsAdmin = ()=>{ setIsAdmin(!isAdmin); }
    
    return (
        <>
            <NavbarContext.Provider value = {{ isAdmin, toggleIsAdmin}}>
                <Header/>
                <MainNavbar/>
            </NavbarContext.Provider>
            {children}
            <Footer/>
        </>
    );};

export default Layout;