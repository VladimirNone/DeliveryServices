import { Nav, Navbar, NavDropdown, Container, Offcanvas} from 'react-bootstrap';
import { FC } from "react"
import Link from 'next/link';
import styles from '@/styles/Home.module.css'

const VerticalMenuItems: Array<linkPanelItem> = [
  { 
    itemName:"Главная",
    itemHref: "/",
  },
  { 
    itemName:"Акции",
    itemHref: "/",
  },
  { 
    itemName:"Доставка",
    itemHref: "/",
  },
];

const AdminPanelMenuItems: Array<linkPanelItem> = 
[
  { 
    itemName:"Добавить блюдо",
    itemHref: "/",
  },
  { 
    itemName:"Статистика",
    itemHref: "/",
  },
  { 
    itemName:"Карта",
    itemHref: "/",
  },
];

const Sidebar: FC = () => {
    return (
        <Navbar bg="light" expand="md" style={{maxHeight:"100vw"}} className="h-100">
            <Container fluid className='h-100 align-items-start justify-content-end g-0 mx-auto'>
            <Navbar.Toggle aria-controls='offcanvasNavbar' className={`${styles.sidebarToggle} h-100`}>
                <div className={`${styles.rotate90deg} align-items-center`}>Категории</div>
            </Navbar.Toggle>
                <Navbar.Offcanvas id='offcanvasNavbar' aria-labelledby='offcanvasNavbarLabel' placement="start">
                    <Offcanvas.Header closeButton>
                        <Offcanvas.Title id='offcanvasNavbarLabel'>
                            Категории
                        </Offcanvas.Title>
                    </Offcanvas.Header>
                    <Offcanvas.Body className='g-0'>
                        <Nav className="flex-column">
                            <Nav.Item className='w-100 d-none d-md-block p-2'>
                                <b>Категории</b>
                            </Nav.Item>
                            {VerticalMenuItems.map((value, i) =>
                                (<Nav.Item key={i}>
                                    <Link className="nav-link" href={value.itemHref}>
                                            {value.itemName}
                                    </Link>
                                </Nav.Item>)
                            )}
                            <NavDropdown id='offcanvasNavbarDropdown' title="Админ панель">
                                {AdminPanelMenuItems.map((value, i) => (
                                        <Link key={i} className="nav-link dropdown-item mx-auto" href={value.itemHref}>
                                            {value.itemName}
                                        </Link>
                                ))}
                            </NavDropdown>
                        </Nav>
                    </Offcanvas.Body>
                </Navbar.Offcanvas>
            </Container>
        </Navbar>
  )
};

export default Sidebar;