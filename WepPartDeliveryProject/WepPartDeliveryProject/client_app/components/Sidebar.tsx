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
        <Navbar bg="light" expand="md" className="h-100">
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
                    <Offcanvas.Body className='g-0 justify-content-center'>
                        <Nav className="flex-column">
                            <Nav.Item className='w-100 d-none d-md-block text-center'>
                                <b>Категории</b>
                            </Nav.Item>
                            {VerticalMenuItems.map((value, i) =>
                                (<Nav.Item className='mx-auto' key={i}>
                                    <Link href={value.itemHref} passHref legacyBehavior>
                                        <Nav.Link>
                                            {value.itemName}
                                        </Nav.Link>
                                    </Link>
                                </Nav.Item>)
                            )}
                            <NavDropdown id='offcanvasNavbarDropdown' className='mx-auto' title="Админ панель">
                                {AdminPanelMenuItems.map((value, i) => (
                                    <NavDropdown.Item key={i} className='mx-auto'>
                                        <Link href={value.itemHref} passHref legacyBehavior>
                                            <Nav.Link>
                                            {value.itemName}
                                            </Nav.Link>
                                        </Link>
                                    </NavDropdown.Item>
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