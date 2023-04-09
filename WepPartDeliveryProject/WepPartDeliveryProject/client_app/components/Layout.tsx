import {FC, ReactNode} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  (
        <>
            <Header/>
            <MainNavbar/>
            {children}
            <Footer/>
        </>
    );

export default Layout;