import { Nav, Navbar, NavDropdown, Container, Button, Form } from 'react-bootstrap';
import { ChangeEvent, FC, useState } from "react";
import Link from 'next/link';
import Image from 'next/image';

const HorizontalMenuItems: Array<linkPanelItem> = [
    {
        itemName: "Главная",
        itemHref: "/",
    },
    {
        itemName: "Акции",
        itemHref: "/",
    },
    {
        itemName: "Доставка",
        itemHref: "/",
    },
];

const AdminPanelMenuItems: Array<linkPanelItem> = [
    {
        itemName: "Статистика",
        itemHref: "/admin/statistics",
    },
    {
        itemName: "Блюда",
        itemHref: "/admin/dishes",
    },
    {
        itemName: "Заказы",
        itemHref: "/admin/orders",
    },
    {
        itemName: "Пользователи",
        itemHref: "/admin/users",
    },
];

const KitchenPanelMenuItems: Array<linkPanelItem> = [
    {
        itemName: "Заказы кухни",
        itemHref: "/kitchen/orders",
    },
    {
        itemName: "Сотрудники кухни",
        itemHref: "/kitchen/workers",
    },
];

interface MainNavbarProps { 
    isAdmin: boolean, 
    isKitchenWorker: boolean 
}

const MainNavbar: FC<MainNavbarProps> = ({ isAdmin, isKitchenWorker }) => {
    const [searchValue, setSearchValue] = useState("");

    const changeSearchValue = (e: ChangeEvent<HTMLInputElement>): void => {
        e.preventDefault();
        setSearchValue(e.target.value);
    }

    return (
        <Navbar expand="md" className="upperNavbar">
            <Container>
                <Navbar.Toggle id="navbarToggle" aria-controls="offcanvasNavbar-expand" className="order-1 col-3" style={{color:"#FFF"}}></Navbar.Toggle>
                <Navbar.Collapse id="basic-navbar-nav" className='order-3'>
                    <Nav className="me-auto">
                        {HorizontalMenuItems.map((value, i) =>
                        (<Nav.Item className='mx-auto' key={i}>
                            <Link href={value.itemHref} className='nav-link' style={{color: "#FFF"}}>
                                {value.itemName}
                            </Link>
                        </Nav.Item>)
                        )}
                        {isAdmin &&
                            <NavDropdown className='mx-auto' title="Админ панель" id="nav-dropdown">
                                {AdminPanelMenuItems.map((value, i) => (
                                    <Nav.Item key={i} className='mx-auto'>
                                        <Link href={value.itemHref} className='dropdown-item pt-2 pb-2 nav-link'>
                                            {value.itemName}
                                        </Link>
                                    </Nav.Item>))
                                }
                            </NavDropdown>
                        }
                        {isKitchenWorker &&
                            <NavDropdown className='mx-auto' title="Панель кухни" id="nav-dropdown">
                                {KitchenPanelMenuItems.map((value, i) => (
                                    <Nav.Item key={i} className='mx-auto'>
                                        <Link href={value.itemHref} className='dropdown-item pt-2 pb-2'>
                                            {value.itemName}
                                        </Link>
                                    </Nav.Item>))
                                }
                            </NavDropdown>
                        }
                    </Nav>
                </Navbar.Collapse>
                <Form className="d-flex col-lg-4 col-md-5 col-8 order-md-4 order-2 ">
                    <Form.Control className='w-100' placeholder="Поиск" aria-describedby="basic-addon2" value={searchValue} onChange={changeSearchValue} />
                    <Button variant="outline-secondary" className='ms-1' id="button-addon2">
                        <Link href={searchValue.trim() != "" ? `/search?searchText=${searchValue.toLowerCase().trim()}` : '/'}>
                            <Image src="\loupe.svg" width="20" height="20" alt="Поиск"/>
                        </Link>
                    </Button>
                </Form>
            </Container>
        </Navbar>
    )
};

export default MainNavbar;