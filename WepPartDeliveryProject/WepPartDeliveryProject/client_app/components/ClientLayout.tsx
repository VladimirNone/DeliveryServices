import {FC, ReactNode} from "react"
import MainNavbar from "./Navbar";
import Header from "./Header";
import Footer from "./Footer";
import Sidebar from "./Sidebar";
import { Col, Container, Row } from "react-bootstrap";

type clientLayoutProps = {
    children: ReactNode,
    categories:Array<categoryItem>,
}

const ClientLayout:FC<clientLayoutProps> = ({children, categories}) =>  (
        <>
            <Container fluid="xl" className="row pt-2 mx-auto">
                <Col xs={2} md={3} lg={2} className="g-0 px-1">
                    <Sidebar categories={categories}/> 
                </Col>
                <Col xs={10} md={9} lg={10} className="g-0">
                    {children}
                </Col>
            </Container>
        </>
    );

export default ClientLayout;