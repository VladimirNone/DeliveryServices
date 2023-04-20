import { Nav, Navbar, Container, Offcanvas } from 'react-bootstrap';
import { FC } from "react"
import Link from 'next/link';
import styles from '@/styles/Home.module.css'

const Sidebar: FC<{ categories: Array<categoryItem> }> = ({ categories }) => {

  return (
    <Navbar bg="light" expand="md" style={{ maxHeight: "100vw" }} className="h-100">
      <Container fluid className='h-100 align-items-start justify-content-end g-0 mx-auto'>
        <Navbar.Toggle aria-controls='offcanvasNavbar' className={`${styles.sidebarToggle} p-0 h-100`}>
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
              {categories?.map((value, i) =>
              (<Nav.Item key={i}>
                <Link className="nav-link" href={`/categories/${value.linkName}`} >
                  {value.name}
                </Link>
              </Nav.Item>)
              )}
            </Nav>
          </Offcanvas.Body>
        </Navbar.Offcanvas>
      </Container>
    </Navbar>
  )
};

export default Sidebar;