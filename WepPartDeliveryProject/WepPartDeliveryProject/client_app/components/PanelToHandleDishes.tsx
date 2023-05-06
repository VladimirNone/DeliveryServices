import Link from 'next/link';
import React, { ChangeEvent, FC, useState } from 'react';
import { Button, Col, Dropdown, Form, Image, Row } from 'react-bootstrap';

interface PanelToHandleUsersProps {
    changeSearchedText: (newSearchedText:string) => void, 
}

const PanelToHandleDishes: FC<PanelToHandleUsersProps> = ({ changeSearchedText }) => {
    const [searchValue, setSearchValue] = useState("");

    const changeSearchValue = (e: ChangeEvent<HTMLInputElement>): void => {
        e.preventDefault();
        setSearchValue(e.target.value);
    }

    const handleSearchClick = ():void =>{
        changeSearchedText(searchValue);
    }

    return (
        <>
            <Row className='pe-md-3'>
                <Col xs={12} md='auto' className='mt-2 flex-grow-1'>
                    <Form className="d-flex w-100">
                        <Form.Control className='w-100' placeholder="Поиск по названию и описанию" aria-describedby="basic-addon2" value={searchValue} onChange={changeSearchValue} />
                        <Button variant="outline-secondary" className='ms-1' onClick={handleSearchClick}>
                            <Image src="\loupe.svg" width="20" height="20" alt="Поиск по Id и логину" />
                        </Button>
                    </Form>
                </Col>
                <Col xs={12} md='auto' className='mt-2 g-0'>
                    <Row className='ms-1 d-flex justify-content-center'>
                        <Col xs='auto' className='mt-1 mt-xxl-0'>
                            <Link href={"/admin/createDish"} className={`btn btn-secondary`}>
                                Создать новое блюдо
                            </Link>
                        </Col>
                    </Row>
                </Col>
            </Row>
        </>
    );
}

export default PanelToHandleDishes;

