import Link from "next/link";
import { FC, MouseEvent } from "react"
import { Button, Col, Container, NavbarBrand } from "react-bootstrap";
import styles from '@/styles/Home.module.css'
import { useRouter } from "next/router";
import Image from "next/image";

interface HeaderProps { 
    isAdmin: boolean, 
    isKitchenWorker: boolean,
    isDeliveryMan: boolean,
    isAuthed: boolean, 
    dropJwtToken: () => void
}

const Header: FC<HeaderProps> = ({isAuthed, dropJwtToken, isAdmin, isKitchenWorker, isDeliveryMan}) => {
    const router = useRouter();

    const logoutClickHandler = async (e:MouseEvent):Promise<void> => {
        e.preventDefault();

        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/auth/logout`, {
            method: "POST",
            credentials: 'include',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
            }, 
        });

        if(resp.ok)
        {
            dropJwtToken();
            router.push("/");
        }
        
    }

    const cartButtonVisibility:boolean = !(isAdmin || isKitchenWorker || isDeliveryMan);

    return (
        <Container fluid="xl" className="row pt-2 mx-auto">
            <Col sm={5} md={6} className="d-flex justify-content-center align-items-center">
                <Link href="/">
                    <NavbarBrand>
                        <Image src="/2_logo.svg" alt="logo" width={220} height={80}/>
                    </NavbarBrand>
                </Link>
            </Col>
            <Col sm={7} md={6} className="d-flex justify-content-end align-items-center">
                {cartButtonVisibility &&  <Col xs={isAuthed ? 4 : 6} sm={4} className={`p-1 ${styles.headerButton}`}>
                    <Link href='/cart' className="btn btn-primary w-100">
                        Корзина
                    </Link>
                </Col>}
                { isAuthed ? 
                    <>
                        <Col xs={4} sm={4} className={`p-1 ${styles.headerButton}`}>
                            <Link href='/profile' className="btn btn-primary w-100">
                                Профиль
                            </Link>
                        </Col>
                        <Col xs={4} sm={4} className={`p-1 ${styles.headerButton}`}>
                            <Button className="w-100" onClick={logoutClickHandler}>Выйти</Button>
                        </Col>
                    </>
                    :
                    <Col xs={6} sm={8} className={`p-1 ${styles.headerButton}`}>
                        <Link href='/login' className="btn btn-primary w-100 text-nowrap">
                            Войти/Регистрация
                        </Link>
                    </Col>
                }
            </Col>
        </Container>
    );
}

export default Header;