import { Nav, Navbar, NavDropdown, Container, Button, Form } from 'react-bootstrap';
import { FC } from "react"
import { NavbarContext } from './contexts/Navbar-context';
import Link from 'next/link';
import Image from 'next/image'

const MainNavbar: FC = () => {
  return (
    <Navbar bg="light" expand="md">
      <Container>
        <Navbar.Toggle aria-controls="basic-navbar-nav" className="w-100">Меню</Navbar.Toggle>
        <Navbar.Collapse id="basic-navbar-nav"  >
          <Nav className="me-auto">
            <Nav.Item className='mx-auto'>
              <Link href="/"passHref legacyBehavior>
                <Nav.Link eventKey="1">
                  Главная
                </Nav.Link>
              </Link>
            </Nav.Item>
            <Nav.Item className='mx-auto'>
              <Link href="/" passHref legacyBehavior>
                <Nav.Link eventKey="2">
                  Акции
                </Nav.Link>
              </Link>
            </Nav.Item>
            <Nav.Item className='mx-auto'>
            <Link href="/" passHref legacyBehavior>
              <Nav.Link eventKey="3" disabled>
                Доставка
              </Nav.Link>
              </Link>
            </Nav.Item>
            <NavbarContext.Consumer >
              {({ isAdmin }) => isAdmin &&
                <NavDropdown className='mx-auto' title="Админ панель" id="nav-dropdown">
                  <NavDropdown.Item className='mx-auto' eventKey="4.1">
                      Click here
                  </NavDropdown.Item>
                  <NavDropdown.Item>
                    Еще что-то
                  </NavDropdown.Item>
                </NavDropdown>
              }
            </NavbarContext.Consumer>
          </Nav>
              <Form className="d-flex col-md-4">
                <Form.Control
                  placeholder="Recipient's username"
                  aria-label="Recipient's username"
                  aria-describedby="basic-addon2"
                />
                <Button variant="outline-secondary" id="button-addon2">
                  <Image src="loupe.svg" width="20" height="20" alt="Поиск" />
                </Button>
              </Form>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  )
};

export default MainNavbar;