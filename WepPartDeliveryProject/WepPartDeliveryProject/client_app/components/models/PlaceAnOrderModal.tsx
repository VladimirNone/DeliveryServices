import React, { ChangeEvent, FC, useEffect, useState } from 'react';
import { Form } from 'react-bootstrap';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

interface PlaceAnOrderModelProps {
    show: boolean | undefined,
    commitAction: (userAddress: string, userPhoneNumber: string, comment: string) => void,
    closeModel: () => void,
}

const PlaceAnOrderModal: FC<PlaceAnOrderModelProps> = ({ show, commitAction, closeModel }) => {
    const [address, setAddress] = useState("")
    const [phoneNumber, setPhoneNumber] = useState("")
    const [comment, setComment] = useState("")

    const changeAddress = (e:ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setAddress(newValue);
    }

    const changePhoneNumber = (e:ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setPhoneNumber(newValue);
    }

    const changeComment = (e:ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setComment(newValue);
    }

    const handleCommit = async () => {
        if(address == "" || phoneNumber == "")
        {
            alert("Для оформления заказа необходимо ввести ваш номер телефона и адрес")
            return;
        }
        commitAction(address, phoneNumber, comment)
    }

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getProfileInfo`, {
                credentials: 'include',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                }, 
            });

            if(resp.ok){
                const profileInfo = await resp.json() as {address: string, phoneNumber: string};
                setAddress(profileInfo.address);
                setPhoneNumber(profileInfo.phoneNumber);
            }        
            else{
                alert(await resp.text());
            }
        }
        fetchData();
    }, []);

    return (
        <>
            <Modal show={show} onHide={closeModel} keyboard={false} aria-labelledby="contained-modal-title-vcenter">
                <Modal.Header closeButton>
                    <Modal.Title>Отмена заказа</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3" >
                            <Form.Label>Пожалуйста, введите адрес, по которому необходимо доставить заказ: </Form.Label>
                            <Form.Control type="text" value={address} onChange={changeAddress}/>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Пожалуйста, введите свой номер телефона: </Form.Label>
                            <Form.Control type="tel" placeholder="+7" maxLength={12} value={phoneNumber} onChange={changePhoneNumber}/>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Комментарий к заказу: </Form.Label>
                            <Form.Control as="textarea" rows={2} value={comment} onChange={changeComment}/>
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={closeModel}>
                        Закрыть
                    </Button>
                    <Button variant="primary" onClick={handleCommit}>Оформить заказ</Button>
                </Modal.Footer>
            </Modal>
        </>
    );
}

export default PlaceAnOrderModal;