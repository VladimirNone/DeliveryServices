import Link from "next/link";
import { FC } from "react"
import { Button, Col, Container, NavbarBrand, Nav, Navbar } from "react-bootstrap";
import styles from '@/styles/Home.module.css'

const questionsPanel:footerPanelInfo = 
{
    panelName: "Часто задаваемые вопросы",
    panelItems: [
        {
            itemName: "Как получить бонусы?",
            itemHref: "/",
        },
        {
            itemName: "Как связаться с менеджером?",
            itemHref: "/",
        },
        {
            itemName: "Как стать частью вашей команды?",
            itemHref: "/",
        },
        {
            itemName: "Какие доступны способы оплаты?",
            itemHref: "/",
        },
    ]
}
const informationPanel:footerPanelInfo = 
{
    panelName: "Общая информация",
    panelItems: [
        {
            itemName: "История сервиса",
            itemHref: "/",
        },
        {
            itemName: "Возникли проблемы с сайтом",
            itemHref: "/",
        },
    ]
}

const FooterPanelItem : FC<footerPanelInfo> = (contentPanel:footerPanelInfo) =>{
    return (
        <Navbar bg="light" expand="md">
            <Container>
                <Navbar.Toggle aria-controls="basic-navbar-nav" className="w-100">{contentPanel.panelName}</Navbar.Toggle>
                <Navbar.Collapse id="basic-navbar-nav "  >
                    <Nav className="me-auto flex-column">
                        <Nav.Item className='w-100 d-none d-md-block text-center'>
                            <b>{contentPanel.panelName}</b>
                        </Nav.Item>

                        {contentPanel.panelItems.map((value, i) =>
                            (<Nav.Item className="w-100" key={i}>
                                <Link href={value.itemHref} passHref legacyBehavior>
                                    <Nav.Link eventKey={i}>
                                        {value.itemName}
                                    </Nav.Link>
                                </Link>
                            </Nav.Item>)
                        )}
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

const Footer: FC = () => {
    return (
        <Container fluid="lg" className="row g-0">
            <Col md={4} xs={12} className="p-1 h-100">
                {FooterPanelItem(informationPanel)}
            </Col>
            <Col md={4} xs={12} className="p-1">
                {FooterPanelItem(questionsPanel)}
            </Col>
            <Col md={4} xs={12} className="order-md-first d-flex justify-content-center align-items-center p-1">
                <Link href="/">
                    <NavbarBrand>
                        My Brand!
                    </NavbarBrand>
                </Link>
            </Col>
        </Container>
    );
}

export default Footer;