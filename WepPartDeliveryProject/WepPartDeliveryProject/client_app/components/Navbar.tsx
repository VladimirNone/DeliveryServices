import { Nav, Navbar, NavDropdown, Container, Button, Form } from 'react-bootstrap';
import { FC } from "react"
import { NavbarContext } from './contexts/Navbar-context';
import Link from 'next/link';
import Image from 'next/image'

const HorizontalMenuItems: Array<string> = [
  "Главная",
  "Акции",
  "Доставка",
];

const AdminPanelMenuItems: Array<string> = [
  "Добавить блюдо",
  "Статистика",
  "Карта",
];

const MainNavbar: FC = () => {
  return (
    <Navbar bg="light" expand="md">
      <Container className="g-0">
        <Navbar.Toggle aria-controls="basic-navbar-nav" data-bs-toggle="offcanvas" className="order-1 col-3"></Navbar.Toggle>
        <Navbar.Collapse id="basic-navbar-nav" className='order-3'>
          <Nav className="me-auto">
            
            {HorizontalMenuItems.map((value, i) =>
              (<Nav.Item className='mx-auto' key={i}>
                <Link href="/" passHref legacyBehavior>
                  <Nav.Link eventKey={i}>
                    {value}
                  </Nav.Link>
                </Link>
              </Nav.Item>)
            )}

            <NavbarContext.Consumer >
              {({ isAdmin }) => isAdmin &&
                <NavDropdown className='mx-auto' title="Админ панель" id="nav-dropdown">
                  {AdminPanelMenuItems.map((value, i) => (
                    <NavDropdown.Item key={i} className='mx-auto'>
                      {value}
                    </NavDropdown.Item>))}
                </NavDropdown>
              }
            </NavbarContext.Consumer>
          </Nav>
        </Navbar.Collapse>
        <Form className="d-flex col-lg-4 col-md-5 col-8 order-md-4 order-2 ">
          <Form.Control className='w-100' placeholder="Поиск" aria-describedby="basic-addon2" />
          <Button variant="outline-secondary" id="button-addon2">
            <Image src="loupe.svg" width="20" height="20" alt="Поиск" />
          </Button>
        </Form>
      </Container>
    </Navbar>
  )
};

export default MainNavbar;