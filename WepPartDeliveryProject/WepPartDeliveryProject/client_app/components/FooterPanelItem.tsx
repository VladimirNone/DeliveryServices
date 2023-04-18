import Link from "next/link";
import { FC } from "react"
import { Nav, Container, Navbar } from "react-bootstrap";

const FooterPanelItem : FC<footerPanelInfo> = (contentPanel:footerPanelInfo) =>{
    return (
        <Navbar bg="light" expand="md">
            <Container fluid="xl" className="g-0" >
                <Navbar.Toggle aria-controls="basic-navbar-nav" className="w-100">{contentPanel.panelName}</Navbar.Toggle>
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="me-auto flex-column w-100">
                        <Nav.Item className='w-100 d-none d-md-block text-center'>
                            <b>{contentPanel.panelName}</b>
                        </Nav.Item>

                        {contentPanel.panelItems.map((value, i) =>
                            (<Nav.Item className="w-100" key={i}>
                                <Link className="nav-link" href={value.itemHref}>
                                        {value.itemName}
                                </Link>
                            </Nav.Item>)
                        )}
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default FooterPanelItem;