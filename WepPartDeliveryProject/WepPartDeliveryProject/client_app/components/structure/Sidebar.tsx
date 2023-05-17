import { Nav, Navbar, Container, Offcanvas } from 'react-bootstrap';
import { FC, useEffect, useState } from "react"
import Link from 'next/link';
import styles from '@/styles/Home.module.css'

const Sidebar: FC<{ categories: Array<categoryItem> }> = ({ categories }) => {

    const [isFixed, setIsFixed] = useState(false);

    useEffect(() => {
      const handleScroll = () => {
        let heightOfHeaderAndNavBar = 32 + 56 + window.innerWidth >= 576 ? 88 : 133

        if (window.scrollY > heightOfHeaderAndNavBar && !isFixed) {
          setIsFixed(true);
        } else if (window.scrollY < heightOfHeaderAndNavBar && isFixed) {
          setIsFixed(false);
        }
      };
      window.addEventListener("scroll", handleScroll);
      window.addEventListener("resize", handleScroll);
      return () => {
        window.removeEventListener("scroll", handleScroll);
        window.removeEventListener("resize", handleScroll);
      };
    }, [isFixed]);

    return (
        <Navbar expand="md" style={{ maxHeight: "100vw", minHeight: "23vw", position: (isFixed?"fixed":"static"), top: "0px"}} className="h-100">
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
                            (<Nav.Item key={i} className='w-100 ms-2'>
                                <Link className="nav-link " href={`/categories/${value.linkName}`} >
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