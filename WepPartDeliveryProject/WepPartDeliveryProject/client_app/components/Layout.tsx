import {FC, ReactNode} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  (
        <>
            <Header/>
            <MainNavbar/>
            {children}
        </>
    );

export default Layout;