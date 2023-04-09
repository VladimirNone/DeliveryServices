import {FC, ReactNode} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";
import Sidebar from "./Sidebar";
import { Col, Container, Row } from "react-bootstrap";

type layoutProps = {
    children: ReactNode
}

const Layout:FC<layoutProps> = ({children}) =>  (
        <>
            <Header/>
            <MainNavbar/>
            <Container fluid="xl" className="row pt-2 mx-auto">
                <Col xs={2} md={3} className="g-0 px-1">
                    <Sidebar/>
                </Col>
                <Col className="g-0">
                    {children}
                </Col>
            </Container>
            <Footer/>
        </>
    );

export default Layout;