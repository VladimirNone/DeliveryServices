import Link from "next/link";
import { FC } from "react"
import { Col, Container, NavbarBrand } from "react-bootstrap";
import FooterPanelItem from "../FooterPanelItem";
import Image from "next/image";

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
            itemName: "Действия при возникновении проблем с сайтом",
            itemHref: "/",
        },
        {
            itemName: "Вакансии",
            itemHref: "/",
        },
    ]
}

const Footer: FC = () => {
    return (
        <Container className="row mx-auto">
            <Col md={4} xs={12} className="p-1 h-100">
                {FooterPanelItem(informationPanel)}
            </Col>
            <Col md={4} xs={12} className="p-1">
                {FooterPanelItem(questionsPanel)}
            </Col>
            <Col md={4} xs={12} className="order-md-first d-flex justify-content-center align-items-center p-1">
                <Link href="/">
                    <NavbarBrand>
                        <Image src="/2_logo.svg" alt="logo" width={220} height={80}/>
                    </NavbarBrand>
                </Link>
            </Col>
        </Container>
    );
}

export default Footer;