import React, { ChangeEvent, FC, MouseEvent, SyntheticEvent, useState } from 'react';
import { Button, Col, Dropdown, Form, Image, Row } from 'react-bootstrap';
import Link from 'next/link';

interface PanelToHandleUsersProps {
    roles: string[], 
    blockUsers: () => void, 
    unblockUsers: () => void,
    addRole: (role:string) => void, 
    removeRole: (role: string) => void,
    changeSearchedText: (newSearchedText:string) => void, 
}

const PanelToHandleUsers: FC<PanelToHandleUsersProps> = ({ roles, blockUsers, unblockUsers, addRole, removeRole, changeSearchedText }) => {
    const [searchValue, setSearchValue] = useState("");
    const [selectedRole, setSelectedRole] = useState("Роль");

    const changeSearchValue = (e: ChangeEvent<HTMLInputElement>): void => {
        e.preventDefault();
        setSearchValue(e.target.value);
    }

    const handleSearchClick = ():void =>{
        changeSearchedText(searchValue);
    }

    const handleAddRole = (): void => {
        if(selectedRole != "Роль")
            addRole(selectedRole);
        else
            alert("Вы не выбрали новую роль для пользователей")
    }

    const handleRemoveRole = (): void => {
        if(selectedRole != "Роль")
            removeRole(selectedRole);
        else
            alert("Вы не выбрали старую роль пользователей")
    }

    const handleSelectRole = (eventKey:string|null): void => {
        setSelectedRole(eventKey ?? "Роль");
    }

    const handleBlockUsers = (): void => {
        blockUsers()
    }

    const handleUnblockUsers = (): void => {
        unblockUsers()
    }

    return (
        <>
            <Row className='pe-md-3'>
                <Col xs={12} md='auto' className='mt-2 flex-grow-1'>
                    <Form className="d-flex w-100">
                        <Form.Control className='w-100' placeholder="Поиск по Id и логину" aria-describedby="basic-addon2" value={searchValue} onChange={changeSearchValue} />
                        <Button variant="outline-secondary" className='ms-1' onClick={handleSearchClick}>
                            <Image src="\loupe.svg" width="20" height="20" alt="Поиск по Id и логину" />
                        </Button>
                    </Form>
                </Col>
                <Col xs={12} md='auto' className='mt-2 g-0'>
                    <Row className='ms-1 d-flex justify-content-center'>
                        <Col xs='auto'  className='mt-1 mt-xxl-0'>
                            <Row className='g-0 me-1'>
                                <Col xs='auto' className='g-0 me-1'>
                                    <Dropdown onSelect={handleSelectRole}>
                                        <Dropdown.Toggle variant="success" id="dropdown-basic">
                                            {selectedRole}
                                        </Dropdown.Toggle>
                                        <Dropdown.Menu >
                                            {roles.map((value, i) => <Dropdown.Item eventKey={value} key={i}>{value}</Dropdown.Item>)}
                                        </Dropdown.Menu>
                                    </Dropdown>
                                </Col>
                                <Col xs='auto' className='g-0 me-1'>
                                    <Button onClick={handleAddRole} className={`btn btn-danger`}>
                                        Добавить роль
                                    </Button>
                                </Col>
                                <Col xs='auto' className='g-0 me-1'>
                                    <Button onClick={handleRemoveRole} className={`btn btn-danger`}>
                                        Удалить роль
                                    </Button>
                                </Col>
                            </Row>
                        </Col>

                        <Col xs='auto' className='mt-1 mt-xxl-0'>
                            <Row className='g-0 me-1 d-flex justify-content-center'>
                                <Col xs='auto' className='g-0 me-1'>
                                    <Button onClick={handleBlockUsers} className={`btn btn-danger`}>
                                        Заблокировать
                                    </Button>
                                </Col>
                                <Col xs='auto' className='g-0 me-1'>
                                    <Button onClick={handleUnblockUsers} className={`btn btn-danger`}>
                                        Разблокировать
                                    </Button>
                                </Col>
                            </Row>
                        </Col>
                    </Row>
                </Col>
            </Row>
        </>
    );
}

export default PanelToHandleUsers;

