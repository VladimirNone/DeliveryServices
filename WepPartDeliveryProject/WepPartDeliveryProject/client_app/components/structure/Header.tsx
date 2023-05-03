import Link from "next/link";
import { FC, MouseEvent } from "react"
import { Button, Col, Container, NavbarBrand } from "react-bootstrap";
import styles from '@/styles/Home.module.css'
import { useRouter } from "next/router";

const Header: FC<{isAuthed: boolean, dropJwtToken: () => void}> = ({isAuthed, dropJwtToken}) => {
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
                <div className={`col-${isAuthed ? '4' : '6'} col-sm-4 p-1 ${styles.headerButton}`}>
                    <Link href='/cart'>
                        <Button className="w-100">Корзина</Button>
                    </Link>
                </div>
                { isAuthed ? 
                    <>
                        <Col xs={4} sm={4} className={`p-1 ${styles.headerButton}`}>
                            <Link href='/profile'>
                                <Button className="w-100">Профиль</Button>
                            </Link>
                        </Col>
                        <Col xs={4} sm={4} className={`p-1 ${styles.headerButton}`}>
                            <Button className="w-100" onClick={logoutClickHandler}>Выйти</Button>
                        </Col>
                    </>
                    :
                    <Col xs={6} sm={8} className={`p-1 ${styles.headerButton}`}>
                        <Link href='/login'>
                            <Button className="w-100 text-nowrap">Войти/Регистрация</Button>
                        </Link>
                    </Col>
                }
            </Col>
        </Container>
    );
}

export default Header;