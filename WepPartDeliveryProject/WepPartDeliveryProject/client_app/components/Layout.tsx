import {FC, ReactNode} from "react"

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  (
        <>
            {children}
        </>
    );

export default Layout;