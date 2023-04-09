import Link from "next/link";
import { FC } from "react"
import { Button, Col, Container, NavbarBrand } from "react-bootstrap";
import styles from '@/styles/Home.module.css'

const Header: FC = () => {
    return (
        <Container fluid="xl" className="row pt-2 mx-auto">
            <Col sm={5} md={6} className="d-flex justify-content-center align-items-center">
                <Link href="/">
                    <NavbarBrand>
                        My Brand!
                    </NavbarBrand>
                </Link>
            </Col>
            <Col sm={7} md={6} className="d-flex justify-content-end">
                <div className={`col-6 col-sm-4 p-1 ${styles.headerButton}`}>
                    <Button className="w-100">Корзина</Button>
                </div>
                <div className={`col-6 col-sm-8 p-1 ${styles.headerButton}`}>
                    <Button className="w-100 text-nowrap">Войти/Регистрация</Button>
                </div>
            </Col>
        </Container>
    );
}

export default Header;